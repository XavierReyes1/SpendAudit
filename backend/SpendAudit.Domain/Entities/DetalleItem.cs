namespace SpendAudit.Domain.Entities;
using SpendAudit.Domain.Common;
public class DetalleItem : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid ComprobanteId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public bool EsRestringido { get; set; }
}
