using Confluent.Kafka;
using Events.Application.Background;
using Events.Application.Contracts.Services;
using Events.Application.Contracts.Services.Messaging;
using Events.Application.Services;
using Events.Application.Services.Messaging;
using Events.Domain.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

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

    public static IServiceCollection ConfigureMessaging(this IServiceCollection services, Action<KafkaSettings> configure)
    {
        var kafkaSettings = new KafkaSettings();
        configure(kafkaSettings);

        // 1. Продюсер (Singleton - один коннект на приложение)
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 1
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        // 2. Консьюмер (Transient - фабрика для изолированного потока воркера)
        services.AddTransient<IConsumer<string, string>>(sp =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                GroupId = kafkaSettings.GroupId,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            return new ConsumerBuilder<string, string>(config).Build();
        });

        // 3. Инфраструктурные сервисы обмена
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<IInboxService, InboxService>();

        // 4. Фоновые воркеры 
        services.AddHostedService<Producer>();
        services.AddHostedService<Consumer>();

        return services;
    }
    public static IServiceCollection ConfigureRedisOptions(this IServiceCollection services, IConfiguration configuration) =>
         services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.Section));

    public static IServiceCollection ConfigureRedis(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;

            var redisOptions = new ConfigurationOptions
            {
                EndPoints = { redisSettings.EndPoint },
                Password = redisSettings.Password,
                ConnectTimeout = redisSettings.ConnectTimeout,
                SyncTimeout = redisSettings.SyncTimeout,
                AbortOnConnectFail = redisSettings.AbortOnConnectFail,
            };

            return ConnectionMultiplexer.Connect(redisOptions);
        });

        services.AddSingleton(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        return services;
    }
}

