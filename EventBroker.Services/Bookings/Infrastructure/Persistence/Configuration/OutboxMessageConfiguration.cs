using Bookings.Application.Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configuration;

internal class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("Outbox");

        builder.HasKey(m => m.TraceId);

        builder.Property(m => m.TraceId)
            .ValueGeneratedNever();

        builder.Property(m => m.Type)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.Topic)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(m => m.PartitionKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(m => m.TimeStampUtc)
            .IsRequired();
    }
}
