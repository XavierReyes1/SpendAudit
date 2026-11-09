namespace SpendAudit.Application.Interfaces;

/// <summary>Identidad del usuario actual, leída de los Claims del JWT.</summary>
public interface ICurrentTenantService
{
    Guid? EmpresaId { get; }
    Guid? UsuarioId { get; }
    string? Rol { get; }
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
