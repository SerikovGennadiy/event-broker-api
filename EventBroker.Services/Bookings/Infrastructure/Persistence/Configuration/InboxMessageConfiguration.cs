using Bookings.Application.Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configuration;

internal class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("Inbox");

        builder.HasKey(m => m.TraceId);

        builder.Property(m => m.TraceId)
            .ValueGeneratedNever();

        builder.Property(m => m.Type)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.ReceivedAt)
            .IsRequired();

        builder.Property(m => m.ReadAttempts)
            .IsRequired();
    }
}
