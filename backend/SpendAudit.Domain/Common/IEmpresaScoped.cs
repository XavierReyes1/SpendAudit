namespace SpendAudit.Domain.Common;

/// <summary>
/// Marca a toda entidad transaccional que pertenece a un tenant (empresa).
/// La infraestructura usa esta interfaz para aplicar el Global Query Filter
/// y para asignar EmpresaId automáticamente al guardar.
/// </summary>
public interface IEmpresaScoped
{
    Guid EmpresaId { get; set; }
}
