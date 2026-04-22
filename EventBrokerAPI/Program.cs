using EventBrokerAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureCors();
builder.Services.ConfigureContext();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureActionFilters();
builder.Services.AddControllers();

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.ConfigureExceptionHandler(logger);

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
