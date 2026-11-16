using Microsoft.EntityFrameworkCore;
using SpendAudit.Application.Interfaces;
using SpendAudit.Domain.Entities;
using SpendAudit.Domain.Enums;
using SpendAudit.Infrastructure.Persistence;

namespace SpendAudit.Infrastructure.Repositories;

// Ningún repositorio escribe "WHERE EmpresaId = ..." a mano: el Global Query Filter del DbContext lo hace.
// Solo se usa IgnoreQueryFilters() en las consultas marcadas como "global" en las interfaces.

public class EmpresaRepository : IEmpresaRepository
{
    private readonly SpendAuditDbContext _db;
    public EmpresaRepository(SpendAuditDbContext db) => _db = db;

    public Task<bool> ExisteRtnAsync(string rtn, CancellationToken ct = default) =>
        _db.Empresas.IgnoreQueryFilters().AnyAsync(e => e.RTN == rtn, ct);

    public void Add(Empresa empresa) => _db.Empresas.Add(empresa);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SpendAuditDbContext _db;
    public UsuarioRepository(SpendAuditDbContext db) => _db = db;

    public Task<Usuario?> GetByEmailParaLoginAsync(string email, CancellationToken ct = default) =>
        _db.Usuarios.IgnoreQueryFilters()
            .Include(u => u.Empresa)
            .Include(u => u.Puesto)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default) =>
        _db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct);

    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Usuarios.Include(u => u.Puesto).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<List<Usuario>> GetAllAsync(CancellationToken ct = default) =>
        _db.Usuarios.Include(u => u.Puesto).AsNoTracking().OrderBy(u => u.NombreCompleto).ToListAsync(ct);

    public void Add(Usuario usuario) => _db.Usuarios.Add(usuario);
}

public class PuestoRepository : IPuestoRepository
{
    private readonly SpendAuditDbContext _db;
    public PuestoRepository(SpendAuditDbContext db) => _db = db;

    public Task<List<Puesto>> GetAllAsync(CancellationToken ct = default) =>
        _db.Puestos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);

    public Task<Puesto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Puestos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Puesto>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        _db.Puestos.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default) =>
        _db.Puestos.AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower(), ct);

    public void Add(Puesto puesto) => _db.Puestos.Add(puesto);
}

public class PoliticaRepository : IPoliticaRepository
{
    private readonly SpendAuditDbContext _db;
    public PoliticaRepository(SpendAuditDbContext db) => _db = db;

    public Task<PoliticaGasto?> GetActivaAsync(bool paraEditar = false, CancellationToken ct = default)
    {
        var query = _db.PoliticasGasto
            .Include(p => p.Topes).ThenInclude(t => t.Puesto)
            .Include(p => p.Terminos)
            .AsSplitQuery();

        if (!paraEditar) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(p => p.Activa, ct);
    }

    public void Add(PoliticaGasto politica) => _db.PoliticasGasto.Add(politica);

    public void RemoverDetalle(PoliticaGasto politica)
    {
        _db.TopesPuesto.RemoveRange(politica.Topes);
        _db.TerminosRestringidos.RemoveRange(politica.Terminos);
    }
}

public class ComprobanteRepository : IComprobanteRepository
{
    private readonly SpendAuditDbContext _db;
    public ComprobanteRepository(SpendAuditDbContext db) => _db = db;

    public void Add(Comprobante comprobante) => _db.Comprobantes.Add(comprobante);

    public void Remove(Comprobante comprobante) => _db.Comprobantes.Remove(comprobante);

    public Task<Comprobante?> GetByIdAsync(Guid id, bool paraEditar = false, CancellationToken ct = default)
    {
        var query = _db.Comprobantes
            .Include(c => c.Items)
            .Include(c => c.Hallazgos)
            .AsSplitQuery();

        if (!paraEditar) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<List<Comprobante>> GetAlbumAsync(Guid usuarioId, CancellationToken ct = default) =>
        _db.Comprobantes
            .Include(c => c.Items)
            .Include(c => c.Hallazgos)
            .AsSplitQuery()
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId && c.Estado == EstadoComprobante.Borrador)
            .OrderByDescending(c => c.FechaCarga)
            .ToListAsync(ct);

    public Task<List<Comprobante>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, Guid usuarioId, CancellationToken ct = default) =>
        _db.Comprobantes
            .Include(c => c.Items)
            .Include(c => c.Hallazgos)
            .AsSplitQuery()
            .Where(c => c.UsuarioId == usuarioId && ids.Contains(c.Id))
            .ToListAsync(ct);

    public async Task<HashSet<(string Rtn, string Numero)>> GetFacturasAceptadasAsync(
        IReadOnlyCollection<string> numeros, CancellationToken ct = default)
    {
        // Se filtra por número en SQL y el par (RTN, número) se resuelve en memoria.
        var filas = await _db.Comprobantes
            .AsNoTracking()
            .Where(c => c.Estado == EstadoComprobante.Liquidado
                        && (c.ResultadoAuditoria == ResultadoAuditoria.Verde
                            || c.ResultadoAuditoria == ResultadoAuditoria.Amarillo)
                        && c.NumeroFactura != null && c.ProveedorRTN != null
                        && numeros.Contains(c.NumeroFactura))
            .Select(c => new { c.ProveedorRTN, c.NumeroFactura })
            .ToListAsync(ct);

        return filas.Select(f => (f.ProveedorRTN!, f.NumeroFactura!)).ToHashSet();
    }

    public async Task<Dictionary<DateOnly, decimal>> GetTotalesPorDiaAsync(
        Guid usuarioId, IReadOnlyCollection<DateOnly> fechas, CancellationToken ct = default)
    {
        var filas = await _db.Comprobantes
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId
                        && c.Estado == EstadoComprobante.Liquidado
                        && c.ResultadoAuditoria != ResultadoAuditoria.Rojo
                        && c.FechaEmision != null && c.MontoTotal != null
                        && fechas.Contains(c.FechaEmision.Value))
            .Select(c => new { Fecha = c.FechaEmision!.Value, Monto = c.MontoTotal!.Value })
            .ToListAsync(ct);

        return filas.GroupBy(f => f.Fecha).ToDictionary(g => g.Key, g => g.Sum(x => x.Monto));
    }
}

public class LoteRepository : ILoteRepository
{
    private readonly SpendAuditDbContext _db;
    public LoteRepository(SpendAuditDbContext db) => _db = db;

    public void Add(LoteLiquidacion lote) => _db.LotesLiquidacion.Add(lote);

    public Task<LoteLiquidacion?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.LotesLiquidacion
            .Include(l => l.Usuario)
            .Include(l => l.Comprobantes).ThenInclude(c => c.Items)
            .Include(l => l.Comprobantes).ThenInclude(c => c.Hallazgos)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<List<LoteLiquidacion>> GetAllAsync(Guid? usuarioId, CancellationToken ct = default)
    {
        var query = _db.LotesLiquidacion
            .Include(l => l.Usuario)
            .Include(l => l.Comprobantes)
            .AsSplitQuery()
            .AsNoTracking()
            .AsQueryable();

        if (usuarioId is { } id) query = query.Where(l => l.UsuarioId == id);
        return query.OrderByDescending(l => l.FechaLiquidacion).ToListAsync(ct);
    }
}
