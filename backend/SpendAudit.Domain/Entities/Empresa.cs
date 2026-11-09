using SpendAudit.Domain.Enums;
namespace SpendAudit.Domain.Entities;

/// <summary>El tenant. Es la única tabla sin EmpresaId.</summary>
public class Empresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string RTN { get; set; } = string.Empty;
    public EstadoSuscripcion EstadoSuscripcion { get; set; } = EstadoSuscripcion.Activa;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
