namespace SpendAudit.Application.Features.Gastos.DTOs;

/// <summary>Datos de cabecera de un comprobante que el empleado puede capturar o corregir.</summary>
public interface IDatosComprobanteDto
{
    string? NumeroFactura { get; }
    string? CAI { get; }
    string? ProveedorRTN { get; }
    DateOnly? FechaEmision { get; }
    decimal? MontoTotal { get; }
    decimal? ISV { get; }
}

/// <summary>
/// Entrada de POST /gastos/borrador. La capa API traduce el multipart (IFormFile) a este DTO
/// para que Application no dependa de ASP.NET; el contenido viaja aparte como Stream.
/// </summary>
public class CreateBorradorRequestDto : IDatosComprobanteDto
{
    public string ArchivoNombre { get; set; } = string.Empty;
    public long ArchivoTamano { get; set; }
    public string? NumeroFactura { get; set; }
    public string? CAI { get; set; }
    public string? ProveedorRTN { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public decimal? MontoTotal { get; set; }
    public decimal? ISV { get; set; }
}

/// <summary>PUT: reemplaza los datos editables (un valor nulo limpia el campo).</summary>
public class UpdateBorradorRequestDto : IDatosComprobanteDto
{
    public string? NumeroFactura { get; set; }
    public string? CAI { get; set; }
    public string? ProveedorRTN { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public decimal? MontoTotal { get; set; }
    public decimal? ISV { get; set; }
}

public class HallazgoDto
{
    public string Regla { get; set; } = string.Empty;
    public string Severidad { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
}

public class DetalleItemDto
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public bool EsRestringido { get; set; }
}

public class ComprobanteResponseDto
{
    public Guid Id { get; set; }
    public string TipoArchivo { get; set; } = string.Empty;
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    /// <summary>Ruta autenticada para descargar el archivo (GET /gastos/{id}/archivo).</summary>
    public string UrlArchivo { get; set; } = string.Empty;
    public string? NumeroFactura { get; set; }
    public string? CAI { get; set; }
    public string? ProveedorRTN { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public decimal? MontoTotal { get; set; }
    public decimal? ISV { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ResultadoAuditoria { get; set; }
    public DateTime FechaCarga { get; set; }
    public List<DetalleItemDto> Items { get; set; } = new();
    public List<HallazgoDto> Hallazgos { get; set; } = new();
}

public class LiquidarLoteRequestDto
{
    public List<Guid> ComprobanteIds { get; set; } = new();
    public string? Descripcion { get; set; }
}

public class ResultadoLoteDto
{
    public Guid LoteId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaLiquidacion { get; set; }
    public int TotalVerdes { get; set; }
    public int TotalAmarillos { get; set; }
    public int TotalRojos { get; set; }
    public List<ComprobanteResponseDto> Comprobantes { get; set; } = new();
}

public class LoteResumenDto
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaLiquidacion { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public int TotalComprobantes { get; set; }
    public int TotalVerdes { get; set; }
    public int TotalAmarillos { get; set; }
    public int TotalRojos { get; set; }
}

public record ArchivoDescarga(Stream Contenido, string ContentType, string NombreArchivo);
