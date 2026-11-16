using FluentValidation;
using FluentValidation.Results;
using SpendAudit.Application.Common.Exceptions;
using SpendAudit.Application.Features.Usuarios.DTOs;
using SpendAudit.Application.Interfaces;
using SpendAudit.Domain.Entities;
using SpendAudit.Domain.Enums;

namespace SpendAudit.Application.Features.Usuarios.Services;

public interface IUsuariosService
{
    Task<List<PuestoResponseDto>> ListarPuestosAsync(CancellationToken ct = default);
    Task<PuestoResponseDto> CrearPuestoAsync(CreatePuestoRequestDto dto, CancellationToken ct = default);
    Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken ct = default);
    Task<UsuarioDto> CrearUsuarioAsync(CreateUsuarioRequestDto dto, CancellationToken ct = default);
}

public class UsuariosService : IUsuariosService
{
    private readonly IPuestoRepository _puestos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<CreatePuestoRequestDto> _puestoValidator;
    private readonly IValidator<CreateUsuarioRequestDto> _usuarioValidator;

    public UsuariosService(
        IPuestoRepository puestos, IUsuarioRepository usuarios, IUnitOfWork uow, IPasswordHasher hasher,
        IValidator<CreatePuestoRequestDto> puestoValidator, IValidator<CreateUsuarioRequestDto> usuarioValidator)
    {
        _puestos = puestos;
        _usuarios = usuarios;
        _uow = uow;
        _hasher = hasher;
        _puestoValidator = puestoValidator;
        _usuarioValidator = usuarioValidator;
    }

    public async Task<List<PuestoResponseDto>> ListarPuestosAsync(CancellationToken ct = default)
    {
        var puestos = await _puestos.GetAllAsync(ct);

        return puestos
            .OrderBy(p => p.Nombre)
            .Select(MapearPuesto)
            .ToList();
    }

    public async Task<PuestoResponseDto> CrearPuestoAsync(CreatePuestoRequestDto dto, CancellationToken ct = default)
    {
        await _puestoValidator.ValidateAndThrowAsync(dto, ct);

        var nombre = dto.Nombre.Trim();

        if (await _puestos.ExisteNombreAsync(nombre, ct))
            throw new ConflictException("Ya existe un puesto con ese nombre en tu empresa.");

        var puesto = new Puesto { Nombre = nombre };

        _puestos.Add(puesto);
        await _uow.SaveChangesAsync(ct);

        return MapearPuesto(puesto);
    }

    public async Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken ct = default)
    {
        var usuarios = await _usuarios.GetAllAsync(ct);

        return usuarios
            .OrderBy(u => u.NombreCompleto)
            .Select(u => MapearUsuario(u, u.Puesto?.Nombre))
            .ToList();
    }

    public async Task<UsuarioDto> CrearUsuarioAsync(CreateUsuarioRequestDto dto, CancellationToken ct = default)
    {
        await _usuarioValidator.ValidateAndThrowAsync(dto, ct);

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _usuarios.ExisteEmailAsync(email, ct))
            throw new ConflictException("Ya existe un usuario registrado con ese correo.");

        Puesto? puesto = null;
        if (dto.PuestoId is Guid puestoId)
        {
            puesto = await _puestos.GetByIdAsync(puestoId, ct)
                ?? throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(CreateUsuarioRequestDto.PuestoId), "El puesto indicado no existe en tu empresa.")
                });
        }

        var usuario = new Usuario
        {
            PuestoId = puesto?.Id,
            NombreCompleto = dto.NombreCompleto.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password),
            Rol = Enum.Parse<RolUsuario>(dto.Rol.Trim(), ignoreCase: true)
        };

        _usuarios.Add(usuario);
        await _uow.SaveChangesAsync(ct);

        return MapearUsuario(usuario, puesto?.Nombre);
    }

    private static PuestoResponseDto MapearPuesto(Puesto puesto) => new()
    {
        Id = puesto.Id,
        Nombre = puesto.Nombre
    };

    private static UsuarioDto MapearUsuario(Usuario usuario, string? puesto) => new()
    {
        Id = usuario.Id,
        NombreCompleto = usuario.NombreCompleto,
        Email = usuario.Email,
        Rol = usuario.Rol.ToString(),
        EmpresaId = usuario.EmpresaId,
        Puesto = puesto
    };
}
