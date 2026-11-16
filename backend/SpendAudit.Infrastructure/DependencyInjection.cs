using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendAudit.Application.Interfaces;
using SpendAudit.Infrastructure.Persistence;
using SpendAudit.Infrastructure.Repositories;
using SpendAudit.Infrastructure.Services;

namespace SpendAudit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'ConnectionStrings__PostgresConnection' (revisa tu archivo .env).");

        services.AddDbContext<SpendAuditDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3)));

        // El DbContext es también la Unidad de Trabajo (un SaveChanges = una transacción)
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SpendAuditDbContext>());

        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPuestoRepository, PuestoRepository>();
        services.AddScoped<IPoliticaRepository, PoliticaRepository>();
        services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
        services.AddScoped<ILoteRepository, LoteRepository>();

        var jwt = config.GetSection(JwtSettings.Seccion).Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
            throw new InvalidOperationException(
                "Jwt__Key debe tener al menos 32 caracteres. Genera una con: openssl rand -base64 48");
        services.AddSingleton(jwt);

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IExtractorTextoService, PdfPigExtractorService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
