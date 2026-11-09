using System.Security.Claims;
using SpendAudit.Application.Interfaces;

namespace SpendAudit.API;

public sealed class HttpCurrentTenantService(IHttpContextAccessor httpContextAccessor)
    : ICurrentTenantService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? EmpresaId => ParseGuid(FindClaim("empresaId", "EmpresaId"));

    public Guid? UsuarioId =>
        ParseGuid(FindClaim(ClaimTypes.NameIdentifier, "usuarioId", "sub"));

    public string? Rol => FindClaim(ClaimTypes.Role, "role");

    private string? FindClaim(params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = User?.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}
