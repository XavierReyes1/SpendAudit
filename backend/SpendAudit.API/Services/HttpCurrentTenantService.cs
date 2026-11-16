using System.Security.Claims;
using SpendAudit.Application.Common.Models;
using SpendAudit.Application.Interfaces;

namespace SpendAudit.API.Services;

public sealed class HttpCurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenantService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? EmpresaId => ParseGuid(User?.FindFirst(ClaimsNombres.EmpresaId)?.Value);

    public Guid? UsuarioId => ParseGuid(User?.FindFirst(ClaimsNombres.UsuarioId)?.Value);

    public string? Rol => User?.FindFirst(ClaimsNombres.Rol)?.Value;

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}
