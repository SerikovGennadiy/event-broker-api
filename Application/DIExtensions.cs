using Application.Services;
using Application.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Background;

namespace Application;

public static class DIExtensions
{
    public static IServiceCollection ConfigureAPIServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }

    public static IServiceCollection ConfigureBackgroundServices(this IServiceCollection services) =>
        services.AddHostedService<BookingHandler>();
}
