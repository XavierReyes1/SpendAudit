using SpendAudit.Domain.Enums;
using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;
public class Comprobante : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? LoteId { get; set; }

    public TipoArchivo TipoArchivo { get; set; }
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    /// <summary>Clave interna de almacenamiento (no es una URL pública).</summary>
    public string UrlArchivo { get; set; } = string.Empty;

    public string? NumeroFactura { get; set; }
    public string? CAI { get; set; }
    public string? ProveedorRTN { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public decimal? MontoTotal { get; set; }
    public decimal? ISV { get; set; }
    public string? TextoExtraido { get; set; }

    public EstadoComprobante Estado { get; set; } = EstadoComprobante.Borrador;
    /// <summary>Instantánea de la auditoría. Nulo mientras sea Borrador.</summary>
    public ResultadoAuditoria? ResultadoAuditoria { get; set; }
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
    public LoteLiquidacion? Lote { get; set; }
    public List<DetalleItem> Items { get; set; } = new();
    public List<HallazgoAuditoria> Hallazgos { get; set; } = new();
}