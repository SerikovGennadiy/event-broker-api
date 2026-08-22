using Application;
using Application.Contracts.Persistance;
using Application.Contracts.Services;
using Application.Contracts.Services.Auth;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Services = Application.Services;

namespace EventBrokerAPI.Tests.BookingService;
public class Fixture : IAsyncLifetime
{
    public ServiceProvider serviceProvider { get; private set; } = null!;
    // Фабричный метод для создания провайдера с возможностью подмены
    public ServiceProvider BuildServiceProvider(Action<IServiceCollection>? overrideServices = null)
    {
        var services = new ServiceCollection();

        // Базовые регистрации
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"BookDB_{Guid.CreateVersion7()}"));
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IEventService, Services.EventService>();
        services.AddScoped<IBookingService, Services.BookingService>();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
        });

        overrideServices?.Invoke(services);

        return services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        // Можно создать дефолтный провайдер
        serviceProvider = BuildServiceProvider();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (serviceProvider != null)
            await serviceProvider.DisposeAsync();
    }

    public void RecreateDatabase()
    {
        var context = serviceProvider!.GetRequiredService<AppDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    // универсальный хелпер для создания сущностей в базе данных
    public async Task<T> CreateEntityAsync<T>(T entity, Action<T>? configure = null) where T : class
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        //var entity = new T();
        configure?.Invoke(entity);

        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    private ICurrentUserService SetuptUserContext(Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);
        currentUser.Setup(x => x.IsAuthenticated).Returns(true);

        return currentUser.Object;
    }

    public async Task InitProviderWithUserContext(Guid userId)
    {
        await CreateEntityAsync<User>(User.Restore(userId, RandomUserName(), ""));
        this.serviceProvider = BuildServiceProvider(provider =>
        {
            provider.AddScoped<ICurrentUserService>(_ => SetuptUserContext(userId));
        });
    }

    private string RandomUserName()
    {
        return $"User_{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}
