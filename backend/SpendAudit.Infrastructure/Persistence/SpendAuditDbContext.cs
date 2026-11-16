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

    /// <summary>
    /// Se evalúa en CADA consulta con el tenant del request actual (EF parametriza el acceso a esta propiedad).
    /// Sin sesión es null, y "EmpresaId == null" nunca es verdadero: sin tenant no se ve ninguna fila.
    /// </summary>
    private Guid? EmpresaIdActual => _tenant.EmpresaId;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        ConfigurarEmpresa(mb);
        ConfigurarUsuarios(mb);
        ConfigurarPoliticas(mb);
        ConfigurarComprobantes(mb);

        // Todas las llaves se generan en la aplicación (Guid.NewGuid()). Así EF trata como "nuevas"
        // las entidades agregadas por navegación en lugar de intentar un UPDATE.
        foreach (var entity in mb.Model.GetEntityTypes())
        {
            mb.Entity(entity.ClrType).Property("Id").ValueGeneratedNever();
        }

        // ---- AISLAMIENTO MULTI-EMPRESA: Global Query Filter en cada entidad con EmpresaId ----
        foreach (var entity in mb.Model.GetEntityTypes()
                     .Where(t => typeof(IEmpresaScoped).IsAssignableFrom(t.ClrType)))
        {
            typeof(SpendAuditDbContext)
                .GetMethod(nameof(AplicarFiltroTenant), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entity.ClrType)
                .Invoke(this, new object[] { mb });
        }

        // Un usuario solo "ve" su propia empresa.
        mb.Entity<Empresa>().HasQueryFilter(e => e.Id == EmpresaIdActual);
    }

    private void AplicarFiltroTenant<T>(ModelBuilder mb) where T : class, IEmpresaScoped
    {
        mb.Entity<T>().HasQueryFilter(e => e.EmpresaId == EmpresaIdActual);
        mb.Entity<T>().HasIndex(e => e.EmpresaId);
    }

    // ------------------------------------------------------------ guardado
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

    /// <summary>
    /// Asigna EmpresaId a lo que se inserta y bloquea cualquier intento de escribir en otra empresa.
    /// El registro de una empresa nueva es el único caso sin sesión: ahí el EmpresaId viene explícito.
    /// </summary>
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

    // ------------------------------------------------------------ configuración por entidad
    private static void ConfigurarEmpresa(ModelBuilder mb)
    {
        mb.Entity<Empresa>(e =>
        {
            e.ToTable("Empresas");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreRazonSocial).HasMaxLength(200).IsRequired();
            e.Property(x => x.RTN).HasMaxLength(14).IsRequired();
            e.Property(x => x.EstadoSuscripcion).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.RTN).IsUnique();
        });
    }

    private static void ConfigurarUsuarios(ModelBuilder mb)
    {
        mb.Entity<Puesto>(e =>
        {
            e.ToTable("Puestos");
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EmpresaId, x.Nombre }).IsUnique();
        });

        mb.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios", t => t.HasCheckConstraint("CK_Usuarios_Rol", "\"Rol\" IN ('Admin','Empleado','Auditor')"));
            e.Property(x => x.NombreCompleto).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Puesto).WithMany().HasForeignKey(x => x.PuestoId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.Email).IsUnique(); // global: el login no pide indicar la empresa
            e.HasIndex(x => new { x.EmpresaId, x.Rol });
        });
    }

    private static void ConfigurarPoliticas(ModelBuilder mb)
    {
        mb.Entity<PoliticaGasto>(e =>
        {
            e.ToTable("PoliticasGasto", t => t.HasCheckConstraint(
                "CK_PoliticasGasto_Vigencia", "\"DiasVigenciaMaxima\" BETWEEN 1 AND 365"));
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            // Una sola política activa por empresa
            e.HasIndex(x => x.EmpresaId).IsUnique().HasFilter("\"Activa\" = TRUE").HasDatabaseName("UX_PoliticasGasto_Activa");
        });

        mb.Entity<TopePuesto>(e =>
        {
            e.ToTable("TopesPuesto", t => t.HasCheckConstraint("CK_TopesPuesto_Tope", "\"TopeDiarioHNL\" > 0"));
            e.Property(x => x.TopeDiarioHNL).HasPrecision(18, 2);
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PoliticaGasto>().WithMany(p => p.Topes).HasForeignKey(x => x.PoliticaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Puesto).WithMany().HasForeignKey(x => x.PuestoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.PoliticaId, x.PuestoId }).IsUnique();
        });

        mb.Entity<TerminoRestringido>(e =>
        {
            e.ToTable("TerminosRestringidos");
            e.Property(x => x.Termino).HasMaxLength(60).IsRequired();
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PoliticaGasto>().WithMany(p => p.Terminos).HasForeignKey(x => x.PoliticaId).OnDelete(DeleteBehavior.Cascade);
            // El servicio guarda los términos en minúsculas, así este índice evita duplicados sin importar el caso.
            e.HasIndex(x => new { x.PoliticaId, x.Termino }).IsUnique();
        });
    }

    private static void ConfigurarComprobantes(ModelBuilder mb)
    {
        mb.Entity<LoteLiquidacion>(e =>
        {
            e.ToTable("LotesLiquidacion");
            e.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EmpresaId, x.UsuarioId, x.FechaLiquidacion });
        });

        mb.Entity<Comprobante>(e =>
        {
            e.ToTable("Comprobantes", t =>
            {
                t.HasCheckConstraint("CK_Comprobantes_Monto", "\"MontoTotal\" IS NULL OR \"MontoTotal\" >= 0");
                t.HasCheckConstraint("CK_Comprobantes_Estado", "\"Estado\" IN ('Borrador','Liquidado')");
                t.HasCheckConstraint("CK_Comprobantes_Resultado",
                    "\"ResultadoAuditoria\" IS NULL OR \"ResultadoAuditoria\" IN ('Verde','Amarillo','Rojo')");
            });
            e.Property(x => x.TipoArchivo).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ResultadoAuditoria).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.NombreArchivoOriginal).HasMaxLength(255).IsRequired();
            e.Property(x => x.UrlArchivo).HasMaxLength(300).IsRequired();
            e.Property(x => x.NumeroFactura).HasMaxLength(30);
            e.Property(x => x.CAI).HasMaxLength(40);
            e.Property(x => x.ProveedorRTN).HasMaxLength(14);
            e.Property(x => x.MontoTotal).HasPrecision(18, 2);
            e.Property(x => x.ISV).HasPrecision(18, 2);
            e.Property(x => x.TextoExtraido).HasColumnType("text");

            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Lote).WithMany(l => l.Comprobantes).HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.SetNull);

            // Búsqueda de duplicados
            e.HasIndex(x => new { x.EmpresaId, x.ProveedorRTN, x.NumeroFactura }).HasDatabaseName("IX_Comprobantes_Factura");
            // Nunca dos facturas aceptadas (Verde/Amarillo) iguales en la misma empresa. Una duplicada
            // sí puede existir como Borrador o como Rojo, por eso el índice único es parcial.
            e.HasIndex(x => new { x.EmpresaId, x.ProveedorRTN, x.NumeroFactura })
                .IsUnique()
                .HasFilter("\"ResultadoAuditoria\" IN ('Verde','Amarillo')")
                .HasDatabaseName("UX_Comprobantes_FacturaAceptada");
            // Álbum del usuario y total diario para el tope
            e.HasIndex(x => new { x.EmpresaId, x.UsuarioId, x.Estado });
            e.HasIndex(x => new { x.EmpresaId, x.UsuarioId, x.FechaEmision });
        });

        mb.Entity<DetalleItem>(e =>
        {
            e.ToTable("DetalleItems");
            e.Property(x => x.Descripcion).HasMaxLength(200).IsRequired();
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Comprobante>().WithMany(c => c.Items).HasForeignKey(x => x.ComprobanteId).OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<HallazgoAuditoria>(e =>
        {
            e.ToTable("HallazgosAuditoria");
            e.Property(x => x.Regla).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Severidad).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Detalle).HasMaxLength(500).IsRequired();
            e.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Comprobante>().WithMany(c => c.Hallazgos).HasForeignKey(x => x.ComprobanteId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
