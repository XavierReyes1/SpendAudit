using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;
public class TerminoRestringido : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid PoliticaId { get; set; }
    public string Termino { get; set; } = string.Empty;
}