using SpendAudit.Domain.Enums;
using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;
public class HallazgoAuditoria : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid ComprobanteId { get; set; }
    public ReglaAuditoria Regla { get; set; }
    public SeveridadHallazgo Severidad { get; set; }
    public string Detalle { get; set; } = string.Empty;
}
