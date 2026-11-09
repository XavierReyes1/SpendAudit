using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;

public class Puesto : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
