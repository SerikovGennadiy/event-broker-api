using Moq;
using AutoMapper;
using Contracts.Repository;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Service;
using Repository;
using Microsoft.EntityFrameworkCore;

namespace EventBrokerAPI.Tests.Fixture.EventService;
public class EventServiceFixture : IDisposable
{
    public required IServiceProvider serviceProvider;

    public EventServiceFixture()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.CreateVersion7()}"));
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IEventService, Service.EventService>();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}