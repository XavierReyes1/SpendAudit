using SpendAudit.Domain.Common;
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