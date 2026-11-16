using SpendAudit.Domain.Entities;
using SpendAudit.Domain.Enums;

namespace SpendAudit.Application.Interfaces;

public interface ICurrentTenantService
{
    Guid? EmpresaId { get; }
    Guid? UsuarioId { get; }
    string? Rol { get; }
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string passwordPlano);
    bool Verify(string passwordPlano, string hash);
}

public interface IJwtTokenService
{
    TokenGenerado Generar(Usuario usuario);
}

public record TokenGenerado(string Token, DateTime ExpiraEn);

public interface IExtractorTextoService
{
    Task<string?> ExtraerTextoAsync(Stream contenido, TipoArchivo tipo, CancellationToken ct = default);
}

public interface IFileStorageService
{
    Task<string> GuardarAsync(Stream contenido, string extension, Guid empresaId, Guid usuarioId, CancellationToken ct = default);
    Task<Stream?> AbrirAsync(string clave, CancellationToken ct = default);
    Task EliminarAsync(string clave, CancellationToken ct = default);
}