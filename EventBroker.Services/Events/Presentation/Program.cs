using Events.API.Extensions;
using Events.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureAPI(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Описываем схему Bearer авторизации
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme // Строка "Bearer"
                }
            }] = Array.Empty<string>()
        };
        document.Servers = new List<OpenApiServer>();

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(), // "bearer"
            BearerFormat = "JWT",
            Description = "Insert JWT-token arrived from Users service"
        };

        document.SecurityRequirements = new List<OpenApiSecurityRequirement> { requirement };
        return Task.CompletedTask;
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase().Run();