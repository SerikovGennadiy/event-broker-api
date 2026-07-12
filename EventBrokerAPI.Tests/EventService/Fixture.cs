using Application;
using Application.Contracts.Persistance;
using Application.Contracts.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services = Application.Services;

namespace EventBrokerAPI.Tests.EventService;
public class Fixture : IAsyncLifetime
{
    public required ServiceProvider serviceProvider;
    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.CreateVersion7()}"));

        services.AddScoped<IEventService, Services.EventService>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddLogging(builder =>
        {
            builder.AddConsole();           // Для вывода в консоль
            builder.AddDebug();             // Для вывода в Debug
            builder.AddFilter("Microsoft", LogLevel.Warning); // Фильтры
            builder.AddFilter("System", LogLevel.Warning);
        });

        serviceProvider = services.BuildServiceProvider();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await serviceProvider.DisposeAsync();
    }

    public void RecreateDatabase()
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}