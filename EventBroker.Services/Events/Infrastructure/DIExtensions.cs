using Events.Application.Contracts.Persistence;
using Events.Infrastructure.Persistence;
using Events.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure;

public static class DIExtensions
{
    public static IServiceCollection ConfigureContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            var configuration = AppDbContextFactory.GetConfigurationFromProject("Presentation");

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения DefaultConnection не найдена");

            opts.UseNpgsql(connectionString, m => m.MigrationsAssembly("Events.Infrastructure"));
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }

    public static IServiceCollection ConfigureRepositoryManager(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }
}
