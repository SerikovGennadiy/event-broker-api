using Microsoft.EntityFrameworkCore;
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

    [Obsolete("Используйте ConfigureAPIServices вместо ConfigureServiceManager")]
    public static IServiceCollection ConfigureServiceManager(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();
        return services;
    }

    public static IServiceCollection ConfigureAPIServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
    public static IServiceCollection ConfigureContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Строка подключения DefaultConnection не найдена");
            opts.UseNpgsql(connectionString, m => m.MigrationsAssembly("EventBrokerAPI"));
            //.LogTo(Console.WriteLine, LogLevel.Information) 
            //.EnableDetailedErrors()                         
            //.EnableSensitiveDataLogging();
        });

        return services;
    }

    public static IServiceCollection ConfigureActionFilters(this IServiceCollection services) =>
        services.AddScoped<ValidateDTOFilter>();

    public static IServiceCollection ConfigureBackgroundServices(this IServiceCollection services) =>
        services.AddHostedService<BookingProcessor.Handler>();
}
