using FluentValidation;
using SpendAudit.Application.Common.Exceptions;
using SpendAudit.Application.Features.Auth.DTOs;
using SpendAudit.Application.Features.Usuarios.DTOs;
using SpendAudit.Application.Interfaces;
using SpendAudit.Domain.Entities;
using SpendAudit.Domain.Enums;

namespace SpendAudit.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<LoginResponseDto> RegistrarEmpresaAsync(RegistrarEmpresaRequestDto dto, CancellationToken ct = default);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IEmpresaRepository _empresas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPoliticaRepository _politicas;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly ICurrentTenantService _tenant;
    private readonly IValidator<RegistrarEmpresaRequestDto> _registroValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthService(
        IEmpresaRepository empresas, IUsuarioRepository usuarios, IPoliticaRepository politicas,
        IUnitOfWork uow, IPasswordHasher hasher, IJwtTokenService jwt, ICurrentTenantService tenant,
        IValidator<RegistrarEmpresaRequestDto> registroValidator, IValidator<LoginRequestDto> loginValidator)
    {
        _empresas = empresas;
        _usuarios = usuarios;
        _politicas = politicas;
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
        _tenant = tenant;
        _registroValidator = registroValidator;
        _loginValidator = loginValidator;
    }

    public async Task<LoginResponseDto> RegistrarEmpresaAsync(RegistrarEmpresaRequestDto dto, CancellationToken ct = default)
    {
        await _registroValidator.ValidateAndThrowAsync(dto, ct);

        if (_tenant.EmpresaId is not null)
            throw new ConflictException("Cierra tu sesión actual antes de registrar una nueva empresa.");

        var rtn = dto.RTN.Trim();
        var email = dto.AdminEmail.Trim().ToLowerInvariant();

        if (await _empresas.ExisteRtnAsync(rtn, ct))
            throw new ConflictException("Ya existe una empresa registrada con ese RTN.");
        if (await _usuarios.ExisteEmailAsync(email, ct))
            throw new ConflictException("Ya existe un usuario registrado con ese correo.");

        var empresa = new Empresa
        {
            NombreRazonSocial = dto.NombreRazonSocial.Trim(),
            RTN = rtn
        };
        var admin = new Usuario
        {
            EmpresaId = empresa.Id,
            NombreCompleto = dto.AdminNombre.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(dto.AdminPassword),
            Rol = RolUsuario.Admin
        };
        // Toda empresa nace con una política activa (sin topes ni términos) para poder auditar desde el día 1.
        var politica = new PoliticaGasto { EmpresaId = empresa.Id };

        _empresas.Add(empresa);
        _usuarios.Add(admin);
        _politicas.Add(politica);
        await _uow.SaveChangesAsync(ct); // una sola transacción: o se crea todo o nada

        return CrearRespuesta(admin);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        await _loginValidator.ValidateAndThrowAsync(dto, ct);

        var usuario = await _usuarios.GetByEmailParaLoginAsync(dto.Email.Trim().ToLowerInvariant(), ct);

        // Mensaje genérico: no revelamos si el correo existe.
        if (usuario is null
            || !usuario.Activo
            || usuario.Empresa?.EstadoSuscripcion != EstadoSuscripcion.Activa
            || !_hasher.Verify(dto.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedException("Credenciales inválidas.");
        }

        return CrearRespuesta(usuario);
    }

    private LoginResponseDto CrearRespuesta(Usuario usuario)
    {
        var token = _jwt.Generar(usuario);
        return new LoginResponseDto
        {
            Token = token.Token,
            ExpiraEn = token.ExpiraEn,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                EmpresaId = usuario.EmpresaId,
                Puesto = usuario.Puesto?.Nombre
            }
        };
    }
}