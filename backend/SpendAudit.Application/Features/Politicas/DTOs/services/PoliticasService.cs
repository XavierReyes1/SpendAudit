using FluentValidation;
using FluentValidation.Results;
using SpendAudit.Application.Common.Exceptions;
using SpendAudit.Application.Features.Politicas.DTOs;
using SpendAudit.Application.Interfaces;
using SpendAudit.Domain.Entities;

namespace SpendAudit.Application.Features.Politicas.Services;

public interface IPoliticasService
{
    Task<PoliticaResponseDto> ObtenerAsync(CancellationToken ct = default);
    Task<PoliticaResponseDto> ActualizarAsync(UpdatePoliticaRequestDto dto, CancellationToken ct = default);
}

public class PoliticasService : IPoliticasService
{
    private readonly IPoliticaRepository _politicas;
    private readonly IPuestoRepository _puestos;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdatePoliticaRequestDto> _validator;

    public PoliticasService(
        IPoliticaRepository politicas, IPuestoRepository puestos, IUnitOfWork uow,
        IValidator<UpdatePoliticaRequestDto> validator)
    {
        _politicas = politicas;
        _puestos = puestos;
        _uow = uow;
        _validator = validator;
    }

    public async Task<PoliticaResponseDto> ObtenerAsync(CancellationToken ct = default)
    {
        var politica = await _politicas.GetActivaAsync(paraEditar: false, ct)
            ?? throw new NotFoundException("Tu empresa no tiene una política de gastos activa.");
        return Mapear(politica);
    }

    public async Task<PoliticaResponseDto> ActualizarAsync(UpdatePoliticaRequestDto dto, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(dto, ct);

        var politica = await _politicas.GetActivaAsync(paraEditar: true, ct)
            ?? throw new NotFoundException("Tu empresa no tiene una política de gastos activa.");

        // Todos los puestos deben existir en ESTA empresa (el filtro global descarta los de otras).
        var idsPuestos = dto.Topes.Select(t => t.PuestoId).Distinct().ToList();
        var puestos = await _puestos.GetByIdsAsync(idsPuestos, ct);
        var faltantes = idsPuestos.Except(puestos.Select(p => p.Id)).ToList();
        if (faltantes.Count > 0)
        {
            throw new ValidationException(faltantes.Select(id =>
                new ValidationFailure("Topes", $"El puesto {id} no existe en tu empresa.")));
        }

        // Términos: sin espacios sobrantes, en minúsculas y sin duplicados.
        var terminos = dto.TerminosRestringidos
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();

        _politicas.RemoverDetalle(politica);
        politica.Topes = dto.Topes.Select(t => new TopePuesto
        {
            EmpresaId = politica.EmpresaId,
            PoliticaId = politica.Id,
            PuestoId = t.PuestoId,
            TopeDiarioHNL = decimal.Round(t.TopeDiarioHNL, 2)
        }).ToList();
        politica.Terminos = terminos.Select(t => new TerminoRestringido
        {
            EmpresaId = politica.EmpresaId,
            PoliticaId = politica.Id,
            Termino = t
        }).ToList();
        politica.DiasVigenciaMaxima = dto.DiasVigenciaMaxima;
        politica.FechaActualizacion = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        // Rearmamos la respuesta con los nombres de puesto ya cargados.
        var nombres = puestos.ToDictionary(p => p.Id, p => p.Nombre);
        return new PoliticaResponseDto
        {
            DiasVigenciaMaxima = politica.DiasVigenciaMaxima,
            Topes = politica.Topes
                .Select(t => new TopePuestoDto
                {
                    PuestoId = t.PuestoId,
                    Puesto = nombres.GetValueOrDefault(t.PuestoId),
                    TopeDiarioHNL = t.TopeDiarioHNL
                }).ToList(),
            TerminosRestringidos = terminos
        };
    }

    private static PoliticaResponseDto Mapear(PoliticaGasto p) => new()
    {
        DiasVigenciaMaxima = p.DiasVigenciaMaxima,
        Topes = p.Topes.Select(t => new TopePuestoDto
        {
            PuestoId = t.PuestoId,
            Puesto = t.Puesto?.Nombre,
            TopeDiarioHNL = t.TopeDiarioHNL
        }).ToList(),
        TerminosRestringidos = p.Terminos.Select(t => t.Termino).OrderBy(t => t).ToList()
    };
}
