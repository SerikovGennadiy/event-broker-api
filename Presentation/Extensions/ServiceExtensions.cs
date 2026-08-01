using Application.Contracts.Persistance;
using Domain.Models;
using Domain.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
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

    public static IServiceCollection ConfigureActionFilters(this IServiceCollection services) =>
        services.AddScoped<ValidateDTOFilter>();


    public static IServiceCollection ConfigureJwtConfiguration(this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

    public static IServiceCollection ConfigureHttpAccessor(this IServiceCollection services) =>
        services.AddHttpContextAccessor();

    public static IServiceCollection ConfigureJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind(jwtSettings.Section, jwtSettings);

        var secretKey = jwtSettings.Secret;
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("Сервер не авторизован");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.ValidIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.ValidAudience,

                    ValidateLifetime = true,

                    RoleClaimType = ClaimTypes.Role,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        return services;
    }
}
