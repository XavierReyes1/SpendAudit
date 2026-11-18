namespace SpendAudit.API.Contracts;

/// <summary>
/// Formulario multipart de POST /gastos/borrador. Vive en la capa API porque IFormFile es de ASP.NET;
/// el controlador lo traduce al DTO de Application.
/// </summary>
public class CreateBorradorForm
{
    public IFormFile? Archivo { get; set; }
    /// <summary>Texto del OCR hecho en el navegador (solo se usa con fotos).</summary>
    public string? TextoOcr { get; set; }
    public string? NumeroFactura { get; set; }
    public string? CAI { get; set; }
    public string? ProveedorRTN { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public decimal? MontoTotal { get; set; }
    public decimal? ISV { get; set; }
}
