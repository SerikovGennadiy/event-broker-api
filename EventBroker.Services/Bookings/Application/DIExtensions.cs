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
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) =>
         services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    public static IServiceCollection ConfigureAMQPMessaging(this IServiceCollection services, Action<KafkaSettings> configure)
    {
        var kafkaSettings = new KafkaSettings();
        configure(kafkaSettings);

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 1,
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddTransient<IConsumer<string, string>>(sp =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                GroupId = kafkaSettings.GroupId,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                MaxInFlight = 1
            };

            return new ConsumerBuilder<string, string>(config).Build();
        });

        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<IInboxService, InboxService>();

        return services;
    }

    public static IServiceCollection InitKafkaProducer(this IServiceCollection services) =>
        services.AddHostedService<Producer>();

    public static IServiceCollection InitLKafkaConsumer(this IServiceCollection services) =>
        services.AddHostedService<Consumer>();
}
