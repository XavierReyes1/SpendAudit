using SpendAudit.Domain.Common;
using SpendAudit.Domain.Enums;

namespace SpendAudit.Domain.Entities;

public class LoteLiquidacion : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaLiquidacion { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
    public List<Comprobante> Comprobantes { get; set; } = new();
}

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

public class DetalleItem : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid ComprobanteId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public bool EsRestringido { get; set; }
}

public class HallazgoAuditoria : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid ComprobanteId { get; set; }
    public ReglaAuditoria Regla { get; set; }
    public SeveridadHallazgo Severidad { get; set; }
    public string Detalle { get; set; } = string.Empty;
}
