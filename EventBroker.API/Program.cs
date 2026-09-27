using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 1. конфигурируем настройки YARP из appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddOpenApiForYarp();

// 1.1. Телеметрия шлюза: трейсы в Jaeger (OTLP), метрики для Prometheus.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(source => source.AddService(
        serviceName: "event-broker-api-gateway",
        serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.1"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]!)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

// 2. CORS-политика по умолчанию (обязательно для вызовов с localhost:5000)
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
app.MapPrometheusScrapingEndpoint();

app.Run();
