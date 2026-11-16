using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;

/// <summary>Política de gastos del tenant. Una sola activa por empresa.</summary>
public class PoliticaGasto : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public int DiasVigenciaMaxima { get; set; } = 30;
    public bool Activa { get; set; } = true;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public List<TopePuesto> Topes { get; set; } = new();
    public List<TerminoRestringido> Terminos { get; set; } = new();
}

public class TopePuesto : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid PoliticaId { get; set; }
    public Guid PuestoId { get; set; }
    public decimal TopeDiarioHNL { get; set; }

    public Puesto? Puesto { get; set; }
}

public class TerminoRestringido : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid PoliticaId { get; set; }
    public string Termino { get; set; } = string.Empty;
}
