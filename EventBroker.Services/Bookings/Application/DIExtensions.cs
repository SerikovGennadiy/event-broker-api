using Bookings.Application.Background;
using Bookings.Application.Contracts.Messaging;
using Bookings.Application.Contracts.Services;
using Bookings.Application.Services;
using Bookings.Application.Services.Messaging;
using Bookings.Domain.Options;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Application;

public static class DIExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
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
}
