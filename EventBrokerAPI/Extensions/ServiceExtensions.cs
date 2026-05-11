using Contracts.Repository;
using Contracts.Service;
using Repository;
using Service;

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
                      .AllowAnyHeader()
                      .WithExposedHeaders("X-Pagination");
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureRepositoryManager(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }

    public static IServiceCollection ConfigureServiceManager(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        return services;
    }

    public static IServiceCollection ConfigureContext(this IServiceCollection services)
    {
        services.AddSingleton<RepositoryContext>();
        return services;
    }

    public static IServiceCollection ConfigureActionFilters(this IServiceCollection services) =>
        services.AddScoped<ValidateDTOFilter>();
}
