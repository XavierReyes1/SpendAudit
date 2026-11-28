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
    private readonly IUsuarioRepository _usuarios;
    private readonly IPuestoRepository _puestos;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ICurrentTenantService _tenant;
    private readonly IValidator<CreatePuestoRequestDto> _puestoValidator;
    private readonly IValidator<CreateUsuarioRequestDto> _usuarioValidator;

    public UsuariosService(
        IUsuarioRepository usuarios, IPuestoRepository puestos, IUnitOfWork uow,
        IPasswordHasher hasher, ICurrentTenantService tenant,
        IValidator<CreatePuestoRequestDto> puestoValidator,
        IValidator<CreateUsuarioRequestDto> usuarioValidator)
    {
        _usuarios = usuarios;
        _puestos = puestos;
        _uow = uow;
        _hasher = hasher;
        _tenant = tenant;
        _puestoValidator = puestoValidator;
        _usuarioValidator = usuarioValidator;
    }

    public async Task<List<PuestoResponseDto>> ListarPuestosAsync(CancellationToken ct = default)
    {
        var puestos = await _puestos.GetAllAsync(ct);
        return puestos.Select(p => new PuestoResponseDto { Id = p.Id, Nombre = p.Nombre }).ToList();
    }

    public async Task<PuestoResponseDto> CrearPuestoAsync(CreatePuestoRequestDto dto, CancellationToken ct = default)
    {
        await _puestoValidator.ValidateAndThrowAsync(dto, ct);

        var nombre = dto.Nombre.Trim();
        if (await _puestos.ExisteNombreAsync(nombre, ct))
            throw new ConflictException("Ya existe un puesto con ese nombre en tu empresa.");

        // EmpresaId lo toma el DbContext del Claim del JWT; jamás viene del cliente.
        var puesto = new Puesto { Nombre = nombre };
        _puestos.Add(puesto);
        await _uow.SaveChangesAsync(ct);

        return new PuestoResponseDto { Id = puesto.Id, Nombre = puesto.Nombre };
    }

    public async Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken ct = default)
    {
        var usuarios = await _usuarios.GetAllAsync(ct);
        return usuarios.Select(Mapear).ToList();
    }

    public async Task<UsuarioDto> CrearUsuarioAsync(CreateUsuarioRequestDto dto, CancellationToken ct = default)
    {
        await _usuarioValidator.ValidateAndThrowAsync(dto, ct);

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _usuarios.ExisteEmailAsync(email, ct))
            throw new ConflictException("Ya existe un usuario registrado con ese correo.");

        Puesto? puesto = null;
        if (dto.PuestoId is { } puestoId)
        {
            puesto = await _puestos.GetByIdAsync(puestoId, ct);
            if (puesto is null)
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(dto.PuestoId), "El puesto indicado no existe en tu empresa.")
                });
        }

        var usuario = new Usuario
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password),
            Rol = Enum.Parse<RolUsuario>(dto.Rol, ignoreCase: true),
            PuestoId = puesto?.Id,
            Puesto = puesto
        };
        _usuarios.Add(usuario);
        await _uow.SaveChangesAsync(ct);

        return Mapear(usuario);
    }

    private static UsuarioDto Mapear(Usuario u) => new()
    {
        Id = u.Id,
        NombreCompleto = u.NombreCompleto,
        Email = u.Email,
        Rol = u.Rol.ToString(),
        EmpresaId = u.EmpresaId,
        Puesto = u.Puesto?.Nombre
    };
}