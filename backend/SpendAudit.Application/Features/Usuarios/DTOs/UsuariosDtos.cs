namespace SpendAudit.Application.Features.Usuarios.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public string? Puesto { get; set; }
}

public class CreateUsuarioRequestDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public Guid? PuestoId { get; set; }
}

public class CreatePuestoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
}

public class PuestoResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
