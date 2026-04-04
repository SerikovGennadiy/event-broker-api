using EventBrokerAPI;
using EventBrokerAPI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureCors();
builder.Services.ConfigureContext();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.ConfigureExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
