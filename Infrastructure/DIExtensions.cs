using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Contracts.Persistance;
using Microsoft.Extensions.Configuration;
using Infrastructure.Persistence.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DIExtensions
{
    public static IServiceCollection ConfigureContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
           var configuration = AppDbContextFactory.GetConfigurationFromProject("Presentation");

            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Строка подключения DefaultConnection не найдена");
            opts.UseNpgsql(connectionString, m => m.MigrationsAssembly("Infrastructure"));
        });

        return services;
    }

    public static IServiceCollection ConfigureRepositoryManager(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }
}
