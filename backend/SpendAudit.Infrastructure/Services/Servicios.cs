using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;

using SpendAudit.Application.Common.Models;

using SpendAudit.Application.Interfaces;

using SpendAudit.Domain.Entities;

using SpendAudit.Domain.Enums;

using UglyToad.PdfPig;



namespace SpendAudit.Infrastructure.Services;



// ---------------------------------------------------------------- JWT

public class JwtSettings

{

    public const string Seccion = "Jwt";



    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "SpendAudit";

    public string Audience { get; set; } = "SpendAudit.Clientes";

    public int ExpirationMinutes { get; set; } = 480;

}



public class JwtTokenService : IJwtTokenService

{

    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings) => _settings = settings;



    public TokenGenerado Generar(Usuario usuario)

    {

        // El empresaId viaja como Claim FIRMADO (HS256): nadie puede alterarlo sin la clave del servidor.

        var claims = new List<Claim>

        {

            new(ClaimsNombres.UsuarioId, usuario.Id.ToString()),

            new(ClaimsNombres.EmpresaId, usuario.EmpresaId.ToString()),

            new(ClaimsNombres.Rol, usuario.Rol.ToString()),

            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

        };



        var credenciales = new SigningCredentials(

            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)), SecurityAlgorithms.HmacSha256);



        var expira = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var token = new JwtSecurityToken(

            issuer: _settings.Issuer,

            audience: _settings.Audience,

            claims: claims,

            notBefore: DateTime.UtcNow,

            expires: expira,

            signingCredentials: credenciales);



        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);

    }

}



// ---------------------------------------------------------------- Contraseñas

public class BCryptPasswordHasher : IPasswordHasher

{

    public string Hash(string passwordPlano) => BCrypt.Net.BCrypt.HashPassword(passwordPlano, workFactor: 11);



    public bool Verify(string passwordPlano, string hash)

    {

        try { return BCrypt.Net.BCrypt.Verify(passwordPlano, hash); }

        catch (Exception) { return false; } // hash corrupto → credenciales inválidas, nunca un 500

    }

}



// ---------------------------------------------------------------- Extracción de texto (PDF)

public class PdfPigExtractorService : IExtractorTextoService

{

    public Task<string?> ExtraerTextoAsync(Stream contenido, TipoArchivo tipo, CancellationToken ct = default)

    {

        // Sin OCR en el MVP: las imágenes quedan para captura manual.

        if (tipo != TipoArchivo.Pdf) return Task.FromResult<string?>(null);



        try

        {

            using var copia = new MemoryStream();

            contenido.CopyTo(copia);

            copia.Position = 0;



            using var pdf = PdfDocument.Open(copia);

            var texto = new StringBuilder();



            foreach (var pagina in pdf.GetPages())

            {

                // Reconstruimos las líneas agrupando palabras por altura (Y), de arriba hacia abajo.

                var palabras = pagina.GetWords()

                    .OrderByDescending(w => w.BoundingBox.Bottom)

                    .ThenBy(w => w.BoundingBox.Left)

                    .ToList();



                var lineaActual = new List<UglyToad.PdfPig.Content.Word>();

                double? yLinea = null;



                void CerrarLinea()

                {

                    if (lineaActual.Count == 0) return;

                    texto.AppendLine(string.Join(" ", lineaActual.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

                    lineaActual.Clear();

                }



                foreach (var palabra in palabras)

                {

                    if (yLinea is null || Math.Abs(palabra.BoundingBox.Bottom - yLinea.Value) > 3)

                    {

                        CerrarLinea();

                        yLinea = palabra.BoundingBox.Bottom;

                    }

                    lineaActual.Add(palabra);

                }

                CerrarLinea();

            }



            var resultado = texto.ToString().Trim();

            return Task.FromResult<string?>(resultado.Length == 0 ? null : resultado); // PDF escaneado → null

        }

        catch (Exception)

        {

            return Task.FromResult<string?>(null); // PDF corrupto o protegido: se captura a mano

        }

    }

}



// ---------------------------------------------------------------- Almacenamiento de archivos

/// <summary>

/// Guarda en disco bajo {Storage:RootPath}/{empresaId}/{usuarioId}/{guid}.ext. Los archivos NO se sirven

/// como estáticos: solo se descargan por GET /gastos/{id}/archivo, que respeta el aislamiento por empresa.

/// OJO: en Render (plan gratis) el disco es efímero; ver notas del README para la Fase 5.

/// </summary>

public class LocalFileStorageService : IFileStorageService

{

    private readonly string _raiz;



    public LocalFileStorageService(IConfiguration config)

    {

        var configurada = config["Storage:RootPath"];

        _raiz = Path.GetFullPath(string.IsNullOrWhiteSpace(configurada) ? "uploads" : configurada);

    }



    public async Task<string> GuardarAsync(Stream contenido, string extension, Guid empresaId, Guid usuarioId, CancellationToken ct = default)

    {

        var clave = $"{empresaId}/{usuarioId}/{Guid.NewGuid()}{extension.ToLowerInvariant()}";

        var ruta = Resolver(clave);

        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);



        await using var destino = File.Create(ruta);

        await contenido.CopyToAsync(destino, ct);

        return clave;

    }



    public Task<Stream?> AbrirAsync(string clave, CancellationToken ct = default)

    {

        var ruta = Resolver(clave);

        Stream? stream = File.Exists(ruta) ? File.OpenRead(ruta) : null;

        return Task.FromResult(stream);

    }



    public Task EliminarAsync(string clave, CancellationToken ct = default)

    {

        var ruta = Resolver(clave);

        if (File.Exists(ruta)) File.Delete(ruta);

        return Task.CompletedTask;

    }



    /// <summary>Evita path traversal: la ruta resultante debe quedar dentro de la raíz.</summary>

    private string Resolver(string clave)

    {

        var completa = Path.GetFullPath(Path.Combine(_raiz, clave));

        if (!completa.StartsWith(_raiz + Path.DirectorySeparatorChar, StringComparison.Ordinal))

            throw new InvalidOperationException("Clave de archivo inválida.");

        return completa;

    }

} 