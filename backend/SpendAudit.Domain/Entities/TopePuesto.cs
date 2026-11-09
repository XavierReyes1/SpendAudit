using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;
public class TopePuesto : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid PoliticaId { get; set; }
    public Guid PuestoId { get; set; }
    public decimal TopeDiarioHNL { get; set; }

    public Puesto? Puesto { get; set; }
}
