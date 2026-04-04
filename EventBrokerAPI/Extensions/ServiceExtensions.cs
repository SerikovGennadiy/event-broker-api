using Contracts.Repository;
using Contracts.Service;
using Repository;

namespace EventBrokerAPI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", option =>
            {
                option.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureRepositoryManager(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, IRepositoryManager>();
        return services;
    }

    public static IServiceCollection ConfigureServiceManager(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, IServiceManager>();
        return services;
    }

    public static IServiceCollection ConfigureContext(this IServiceCollection services)
    {
        services.AddSingleton<RepositoryContext>();
        return services;
    }
}
