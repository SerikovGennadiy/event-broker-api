using Confluent.Kafka;
using Events.Application.Contracts.Services;
using Events.Application.Services;
using Events.Domain.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Application;

public static class DIExtensions
{
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) =>
         services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddKafkaInfrastructure(
         this IServiceCollection services,
         Action<KafkaSettings> configureOptions)
    {
        var kafkaSettings = new KafkaSettings();

        // 🟢 МАГИЯ: Запускаем делегат, который заполнит свойства объекта
        configureOptions(kafkaSettings);

        // Инициализируем Singleton Продюсер
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
            };
            return new ProducerBuilder<string, string>(config).Build();
        });

        // Инициализируем Transient Консьюмер
        services.AddTransient<IConsumer<string, string>>(sp =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                GroupId = kafkaSettings.GroupId,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
            return new ConsumerBuilder<string, string>(config).Build();
        });

        return services;
    }
}

