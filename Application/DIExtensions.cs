using Application.Services;
using Application.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Background;

namespace Application;

public static class DIExtensions
{
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) =>
         services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

    public static IServiceCollection ConfigureAPIServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ICurrectUserService, CurrectUserService>();

        return services;
    }

    public static IServiceCollection ConfigureBackgroundServices(this IServiceCollection services) =>
        services.AddHostedService<BookingHandler>();
}
