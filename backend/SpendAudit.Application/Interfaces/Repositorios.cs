using SpendAudit.Domain.Entities;

namespace SpendAudit.Application.Interfaces;

// Los métodos Add/Remove solo marcan el cambio; se confirma con IUnitOfWork.SaveChangesAsync().
// Todas las consultas están filtradas por empresa automáticamente (Global Query Filter),
// salvo las que dicen "global" porque ocurren antes de conocer el tenant.

public interface IEmpresaRepository
{
    /// <summary>Global: verifica el RTN en todas las empresas.</summary>
    Task<bool> ExisteRtnAsync(string rtn, CancellationToken ct = default);
    void Add(Empresa empresa);
}

public interface IUsuarioRepository
{
    /// <summary>Global: el login ocurre antes de conocer el tenant. Incluye Empresa y Puesto.</summary>
    Task<Usuario?> GetByEmailParaLoginAsync(string email, CancellationToken ct = default);
    /// <summary>Global: el email es único en toda la plataforma.</summary>
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Usuario>> GetAllAsync(CancellationToken ct = default);
    void Add(Usuario usuario);
}

public interface IPuestoRepository
{
    Task<List<Puesto>> GetAllAsync(CancellationToken ct = default);
    Task<Puesto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Puesto>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);
    void Add(Puesto puesto);
}

public interface IPoliticaRepository
{
    /// <summary>Política activa con topes (y su puesto) y términos. paraEditar=true la deja rastreada.</summary>
    Task<PoliticaGasto?> GetActivaAsync(bool paraEditar = false, CancellationToken ct = default);
    void Add(PoliticaGasto politica);
    void RemoverDetalle(PoliticaGasto politica);
}

public interface IComprobanteRepository
{
    void Add(Comprobante comprobante);
    void Remove(Comprobante comprobante);
    Task<Comprobante?> GetByIdAsync(Guid id, bool paraEditar = false, CancellationToken ct = default);
    Task<List<Comprobante>> GetAlbumAsync(Guid usuarioId, CancellationToken ct = default);
    /// <summary>Comprobantes del usuario con esos ids (cualquier estado), rastreados para editar.</summary>
    Task<List<Comprobante>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, Guid usuarioId, CancellationToken ct = default);
    /// <summary>Pares (RTN proveedor, número) de facturas ya liquidadas como Verde/Amarillo en la empresa.</summary>
    Task<HashSet<(string Rtn, string Numero)>> GetFacturasAceptadasAsync(IReadOnlyCollection<string> numeros, CancellationToken ct = default);
    /// <summary>Total ya liquidado (no Rojo) del usuario por fecha de emisión.</summary>
    Task<Dictionary<DateOnly, decimal>> GetTotalesPorDiaAsync(Guid usuarioId, IReadOnlyCollection<DateOnly> fechas, CancellationToken ct = default);
}

public interface ILoteRepository
{
    void Add(LoteLiquidacion lote);
    /// <summary>Incluye Usuario y Comprobantes con hallazgos.</summary>
    Task<LoteLiquidacion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Incluye Usuario y Comprobantes. usuarioId=null devuelve todos los de la empresa.</summary>
    Task<List<LoteLiquidacion>> GetAllAsync(Guid? usuarioId, CancellationToken ct = default);
}
