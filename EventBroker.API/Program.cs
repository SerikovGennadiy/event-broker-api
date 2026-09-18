var builder = WebApplication.CreateBuilder(args);

// 1. Инициализируем прокси YARP из appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddOpenApiForYarp();

// 2. Настраиваем глобальный CORS (теперь в микросервисах его можно отключить!)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5000") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.MapReverseProxy();   
app.MapOpenApiForYarp();
app.MapScalarForYarp();

app.Run();
