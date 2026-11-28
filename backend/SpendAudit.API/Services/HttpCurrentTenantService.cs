using SpendAudit.Application.Common.Models;
using SpendAudit.Application.Interfaces;

namespace SpendAudit.API.Services;

/// <summary>
/// Lee la identidad de los Claims del JWT ya validado. Es la ÚNICA fuente del EmpresaId:
/// ningún endpoint lo acepta desde el body, la ruta o el query string.
/// </summary>
public class HttpCurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _http;
    public HttpCurrentTenantService(IHttpContextAccessor http) => _http = http;

    private System.Security.Claims.ClaimsPrincipal? Usuario =>
        _http.HttpContext?.User is { Identity.IsAuthenticated: true } u ? u : null;

    public Guid? EmpresaId => LeerGuid(ClaimsNombres.EmpresaId);
    public Guid? UsuarioId => LeerGuid(ClaimsNombres.UsuarioId);
    public string? Rol => Usuario?.FindFirst(ClaimsNombres.Rol)?.Value;

    private Guid? LeerGuid(string claim) =>
        Guid.TryParse(Usuario?.FindFirst(claim)?.Value, out var id) ? id : null;
}