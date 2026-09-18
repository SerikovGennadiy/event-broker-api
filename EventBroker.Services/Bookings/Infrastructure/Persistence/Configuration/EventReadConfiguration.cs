using Bookings.Infrastructure.Persistence.Messaging.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configurations;

public sealed class EventReadConfiguration : IEntityTypeConfiguration<EventRead>
{
    public void Configure(EntityTypeBuilder<EventRead> builder)
    {
        builder.ToTable("EventReads");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedNever()
               .IsRequired();

        builder.Property(x => x.StartAt)
               .IsRequired();

        // ИНДЕКС: Ускоряет выборку и фильтрацию событий по дате начала (например, для афиши)
        builder.HasIndex(x => x.StartAt)
               .HasDatabaseName("IX_EventReads_StartAt");
    }
}