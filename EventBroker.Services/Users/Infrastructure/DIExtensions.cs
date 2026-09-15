using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Contracts.Persistence;
using Users.Infrastructure.Persistance.Repository;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure;

public static class DIExtensions
{
    public static IServiceCollection ConfigureContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            var configuration = AppDbContextFactory.GetConfigurationFromProject("Users.API");

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения DefaultConnection не найдена");
            opts.UseNpgsql(connectionString, m => m.MigrationsAssembly("Users.Infrastructure"));
        });

        return services;
    }

    public static IServiceCollection ConfigureRepositoryManager(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }
}
