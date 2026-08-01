using Application;
using EventBrokerAPI.Extensions;
using Infrastructure;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureCors();
builder.Services.ConfigureContext(builder.Configuration);
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureAPIServices();
builder.Services.ConfigureActionFilters();
builder.Services.ConfigureBackgroundServices();
builder.Services.ConfigureAutoMapper();

builder.Services.ConfigureJwtConfiguration(builder.Configuration);
builder.Services.ConfigureJwtAuth(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase().Run();
