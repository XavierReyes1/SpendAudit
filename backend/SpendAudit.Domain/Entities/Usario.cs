using SpendAudit.Domain.Enums;
using SpendAudit.Domain.Common;

namespace SpendAudit.Domain.Entities;


public class Usuario : IEmpresaScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Guid? PuestoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;
    public bool Activo { get; set; } = true;

    public Empresa? Empresa { get; set; }
    public Puesto? Puesto { get; set; }
}
