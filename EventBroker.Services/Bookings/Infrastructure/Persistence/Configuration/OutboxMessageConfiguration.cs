using Bookings.Application.Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configuration;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        // ПЕРВИЧНЫЙ КЛЮЧ: суррогатный хронологический UUIDv7.
        // TraceId key быть не может: один TraceId саги переиспользуется
        // на нескольких шагах (BookingStarted -> SeatReserved -> ...),
        // и второе сообщение с тем же TraceId дало бы PK-конфликт.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedNever()
               .IsRequired();

        // TraceId - корреляционный идентификатор саги, не ключ.
        builder.Property(x => x.TraceId)
               .ValueGeneratedNever()
               .IsRequired();

        builder.Property(x => x.PartitionKey)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Type)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Topic)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.Content)
               .HasColumnType("text")
               .IsRequired();

        builder.Property(x => x.Error)
               .HasColumnType("text")
               .IsRequired(false);

        builder.Property(x => x.TimeStampUtc)
               .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
               .IsRequired(false);

        builder.HasIndex(x => new { x.ProcessedAtUtc, x.TimeStampUtc })
               .HasDatabaseName("IX_OutboxMessages_Pending");
    }
}