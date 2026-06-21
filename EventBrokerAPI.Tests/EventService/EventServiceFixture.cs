using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Repository;
using System.Net;
using System.Threading.Tasks;

namespace EventBrokerAPI.Tests.Fixture.EventService;
public class EventServiceFixture : IAsyncLifetime
{
    public required ServiceProvider serviceProvider;
    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.CreateVersion7()}"));

        services.AddScoped<IEventService, Service.EventService>();
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