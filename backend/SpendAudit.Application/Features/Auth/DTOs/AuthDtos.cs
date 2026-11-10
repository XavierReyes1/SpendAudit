using SpendAudit.Application.Features.Usuarios.DTOs;

namespace SpendAudit.Application.Features.Auth.DTOs;

public class RegistrarEmpresaRequestDto
{
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string RTN { get; set; } = string.Empty;
    public string AdminNombre { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}
