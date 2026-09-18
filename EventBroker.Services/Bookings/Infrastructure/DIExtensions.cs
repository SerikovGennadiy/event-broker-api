using Bookings.Application.Contracts.Persistence;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class DIExtensions
{
    public static IServiceCollection ConfigureContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            var configuration = AppDbContextFactory.GetConfigurationFromProject("Presentation");

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения DefaultConnection не найдена");

            opts.UseNpgsql(connectionString, m => m.MigrationsAssembly("Bookings.Infrastructure"));
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
