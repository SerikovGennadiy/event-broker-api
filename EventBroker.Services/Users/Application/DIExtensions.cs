using Microsoft.Extensions.DependencyInjection;
using Users.Application.Contracts.Services;
using Users.Application.Services.Auth;

namespace Users.Application;

public static class DIExtensions
{
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) =>
         services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<ICurrentUserService, CurrectUserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        return services;
    }
}
