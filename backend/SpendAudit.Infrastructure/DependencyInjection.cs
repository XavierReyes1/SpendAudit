using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpendAudit.Application.Interfaces;
using SpendAudit.Infrastructure.Persistence;

namespace SpendAudit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException(
                "La cadena de conexión 'PostgresConnection' no está configurada.");

        services.AddDbContext<SpendAuditDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<SpendAuditDbContext>());

        return services;
    }
}
