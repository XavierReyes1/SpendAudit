using SpendAudit.Application.Features.Gastos.DTOs;
using SpendAudit.Domain.Entities;
using SpendAudit.Domain.Enums;

namespace SpendAudit.Application.Features.Gastos.Mappings;

public static class ComprobanteMapper
{
    public static ComprobanteResponseDto ToDto(Comprobante c) => new()
    {
        Id = c.Id,
        TipoArchivo = c.TipoArchivo.ToString(),
        NombreArchivoOriginal = c.NombreArchivoOriginal,
        UrlArchivo = $"/api/v1/gastos/{c.Id}/archivo",
        NumeroFactura = c.NumeroFactura,
        CAI = c.CAI,
        ProveedorRTN = c.ProveedorRTN,
        FechaEmision = c.FechaEmision,
        MontoTotal = c.MontoTotal,
        ISV = c.ISV,
        Estado = c.Estado.ToString(),
        ResultadoAuditoria = c.ResultadoAuditoria?.ToString(),
        FechaCarga = c.FechaCarga,
        Items = c.Items.Select(i => new DetalleItemDto
        {
            Descripcion = i.Descripcion,
            Subtotal = i.Subtotal,
            EsRestringido = i.EsRestringido
        }).ToList(),
        Hallazgos = c.Hallazgos.Select(h => new HallazgoDto
        {
            Regla = h.Regla.ToString(),
            Severidad = h.Severidad.ToString(),
            Detalle = h.Detalle
        }).ToList()
    };

    public static ResultadoLoteDto ToDto(LoteLiquidacion lote) => new()
    {
        LoteId = lote.Id,
        Descripcion = lote.Descripcion,
        FechaLiquidacion = lote.FechaLiquidacion,
        TotalVerdes = lote.Comprobantes.Count(c => c.ResultadoAuditoria == ResultadoAuditoria.Verde),
        TotalAmarillos = lote.Comprobantes.Count(c => c.ResultadoAuditoria == ResultadoAuditoria.Amarillo),
        TotalRojos = lote.Comprobantes.Count(c => c.ResultadoAuditoria == ResultadoAuditoria.Rojo),
        Comprobantes = lote.Comprobantes.OrderBy(c => c.FechaCarga).Select(ToDto).ToList()
    };

    public static LoteResumenDto ToResumenDto(LoteLiquidacion lote) => new()
    {
        Id = lote.Id,
        Descripcion = lote.Descripcion,
        FechaLiquidacion = lote.FechaLiquidacion,
        Usuario = lote.Usuario?.NombreCompleto ?? string.Empty,
        TotalComprobantes = lote.Comprobantes.Count,
        TotalVerdes = lote.Comprobantes.Count(c => c.ResultadoAuditoria == ResultadoAuditoria.Verde),
        TotalAmarillos = lote.Comprobantes.Count(c => c.ResultadoAuditoria == ResultadoAuditoria.Amarillo),
        TotalRojos = lote.Comprobantes.Count(c => c.ResultadoAuditoria == ResultadoAuditoria.Rojo)
    };
}
