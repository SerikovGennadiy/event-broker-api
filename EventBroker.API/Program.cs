using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Инициализируем прокси YARP из appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Настраиваем глобальный CORS (теперь в микросервисах его можно отключить!)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // URL вашего фронтенда (например, React)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
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
var app = builder.Build();

app.UseCors();

// 3. Делегируем сбор OpenAPI документации: единый Swagger UI шлюза
app.UseSwaggerUI(options =>
{
    // Скачиваем JSON схемы через проксированные YARP-маршруты с внутренних портов сервисов
    options.SwaggerEndpoint("/openapi/users/v1.json", "Микросервис Пользователей (Users/Auth)");
    options.SwaggerEndpoint("/openapi/events/v1.json", "Микросервис Мероприятий (Events)");
    options.SwaggerEndpoint("/openapi/bookings/v1.json", "Микросервис Бронирований (Bookings)");

    // Swagger UI будет доступен по адресу: http://localhost:5000/docs
    options.RoutePrefix = "docs";
});

// 4. Запускаем инверсный прокси YARP
app.MapReverseProxy();

app.Run();