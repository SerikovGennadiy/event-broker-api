using Contracts.Repository;
using Contracts.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repository;

namespace EventBrokerAPI.Tests.BookingService;
public class BookingServiceFixture : IAsyncLifetime
{
    public required ServiceProvider serviceProvider;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        // Регистрируем реальный сервис
        services.AddDbContext<AppDbContext>(options => 
            options.UseInMemoryDatabase($"BookDB_{Guid.CreateVersion7()}"));
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IEventService, Service.EventService>();
        services.AddScoped<IBookingService, Service.BookingService>();

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
