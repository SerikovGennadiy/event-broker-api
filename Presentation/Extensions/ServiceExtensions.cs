using Application;
using Domain.Options;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
namespace EventBrokerAPI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureEventAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi()
                .ConfigureCors()
                .ConfigureContext(configuration)
                .ConfigureRepositoryManager()
                .ConfigureHttpAccessor()
                .ConfigureAuthServices()
                .ConfigureAPIServices()
                .ConfigureActionFilters()
                .ConfigureBackgroundServices()
                .ConfigureAutoMapper()
                .ConfigureJwtConfiguration(configuration)
                .ConfigureJwtAuth(configuration)
                .AddControllers();

        services.AddSwaggerGen(options =>
        {

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Event Broker API",
                Description = "API с JWT аутентификацией для бронирования мероприятий"
            });
            // описательная часть схемы авторизации
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"Использование JWT-токена в заголовке Authorization со схемой Bearer.
                        Введите слово 'Bearer', затем пробел и ваш токен в текстовое поле ниже.
                        Например: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            // требование добавить описанный Header
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
        return services;
    }

    #region основная DI конфигурация сервисов API
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

        services.AddAuthorization();

        return services;
    }
    #endregion

}
