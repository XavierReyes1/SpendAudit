using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SpendAudit.Application.Interfaces;
using SpendAudit.Domain.Common;
using SpendAudit.Domain.Entities;

namespace SpendAudit.Infrastructure.Persistence;

public class SpendAuditDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentTenantService _tenant;

    public SpendAuditDbContext(DbContextOptions<SpendAuditDbContext> options, ICurrentTenantService tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    // DbSets de las 10 Tablas 3FN
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PoliticaGasto> PoliticasGasto => Set<PoliticaGasto>();
    public DbSet<TopePuesto> TopesPuesto => Set<TopePuesto>();
    public DbSet<TerminoRestringido> TerminosRestringidos => Set<TerminoRestringido>();
    public DbSet<LoteLiquidacion> LotesLiquidacion => Set<LoteLiquidacion>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<DetalleItem> DetalleItems => Set<DetalleItem>();
    public DbSet<HallazgoAuditoria> HallazgosAuditoria => Set<HallazgoAuditoria>();

    private Guid? EmpresaIdActual => _tenant.EmpresaId;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Relaciones explícitas de políticas de gastos

        mb.Entity<PoliticaGasto>()
            .HasMany(p => p.Topes)
            .WithOne()
            .HasForeignKey(t => t.PoliticaId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<PoliticaGasto>()
            .HasMany(p => p.Terminos)
            .WithOne()
            .HasForeignKey(t => t.PoliticaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Llaves generadas en aplicación (Guid.NewGuid())
        foreach (var entity in mb.Model.GetEntityTypes())
        {
            mb.Entity(entity.ClrType).Property("Id").ValueGeneratedNever();
        }

        // ---- AISLAMIENTO MULTI-EMPRESA: Global Query Filter ----
        foreach (var entity in mb.Model.GetEntityTypes()
                     .Where(t => typeof(IEmpresaScoped).IsAssignableFrom(t.ClrType)))
        {
            typeof(SpendAuditDbContext)
                .GetMethod(nameof(AplicarFiltroTenant), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entity.ClrType)
                .Invoke(this, new object[] { mb });
        }

        mb.Entity<Empresa>().HasQueryFilter(e => e.Id == EmpresaIdActual);
    }

    private void AplicarFiltroTenant<T>(ModelBuilder mb) where T : class, IEmpresaScoped
    {
        mb.Entity<T>().HasQueryFilter(e => e.EmpresaId == EmpresaIdActual);
        mb.Entity<T>().HasIndex(e => e.EmpresaId);
    }

    // ---- GUARDADO TRANSACCIONAL CON ASIGNACIÓN AUTOMÁTICA ----
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AplicarTenantAlGuardar();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AplicarTenantAlGuardar();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void AplicarTenantAlGuardar()
    {
        var empresaActual = _tenant.EmpresaId;

        foreach (var entry in ChangeTracker.Entries<IEmpresaScoped>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.EmpresaId == Guid.Empty)
                {
                    entry.Entity.EmpresaId = empresaActual
                        ?? throw new InvalidOperationException(
                            $"No se puede guardar {entry.Entity.GetType().Name} sin una empresa en la sesión.");
                }
                else if (empresaActual is not null && entry.Entity.EmpresaId != empresaActual)
                {
                    throw new InvalidOperationException("Intento de escribir datos en una empresa distinta a la de la sesión.");
                }
            }
            else if (entry.State == EntityState.Modified &&
                     entry.Property(nameof(IEmpresaScoped.EmpresaId)).IsModified)
            {
                throw new InvalidOperationException("EmpresaId es inmutable.");
            }
        }
    }
}