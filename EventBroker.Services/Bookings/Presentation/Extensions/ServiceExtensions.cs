using Bookings.Application;
using Bookings.Domain.Options;
using Bookings.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Bookings.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi()
                .ConfigureCors()
                .ConfigureContext(configuration)
                .ConfigureRepositoryManager()
                .ConfigureHttpAccessor()
                .ConfigureServices()
                .ConfigureAutoMapper()
                .ConfigureJwtConfiguration(configuration)
                .ConfigureJwtAuth(configuration)
                .ConfigureMessaging(settings =>
                {
                    settings.BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
                    settings.GroupId = configuration["Kafka:GroupId"] ?? "bookings-service-group";
                })
                .AddControllers();

        return services;
    }

    #region основная DI конфигурация сервисов API
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) =>
         services.AddAutoMapper(
             cfg => { },
             typeof(Bookings.Application.MappingProfile),
             typeof(Bookings.Infrastructure.Persistence.Messaging.ReadModels.MappingProfile));

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

        services.AddAuthorization();

        return services;
    }
    #endregion

    #region Observability (Телеметрия)
    public static IServiceCollection ConfigureObservability(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(source =>
                source.AddService(
                    serviceName: environment.ApplicationName,
                    serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.1"
            ))
            .WithTracing(tracing =>  
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation()
                       .AddOtlpExporter(o => o.Endpoint = new Uri(configuration["Otlp:Endpoint"]!)));
                       
        return services;
    }
    #endregion
}
