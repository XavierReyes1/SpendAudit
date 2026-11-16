using MediatR;
using Microsoft.AspNetCore.Http;

namespace SpendAudit.Application.Features.Gastos.Commands.CrearBorrador;

public record CrearGastoBorradorCommand(
    IFormFile? Archivo,
    // Campos manuales (opcionales)
    decimal? TotalManual,
    decimal? IsvManual,
    string? RtnManual,
    string? CaiManual,
    string? NumeroFacturaManual,
    DateTime? FechaManual,
    string? ProveedorManual
) : IRequest<Guid>;