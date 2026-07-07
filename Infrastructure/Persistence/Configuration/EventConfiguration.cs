using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;

internal class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedNever();

        builder.Property(e => e.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.TotalSeats)
               .IsRequired();

        builder.Property(e => e.AvailableSeats)
               .IsRequired();

        builder.Property(e => e.StartAt)
               .IsRequired();

        builder.Property(e => e.EndAt)
               .IsRequired();

        builder.Property(e => e.Description)
               .HasMaxLength(1000);

        builder.HasMany(e => e.Bookings)
               .WithOne(b => b.Event)
               .HasForeignKey(b => b.EventId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
