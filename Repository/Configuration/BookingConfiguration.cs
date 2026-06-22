using Entities.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configuration;

internal class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
               .ValueGeneratedNever();

        builder.Property(b => b.EventId)
               .IsRequired();
        
        builder.Property(b => b.CreatedAt)
               .IsRequired();
        
        builder.Property(b => b.ProcessedAt);

        builder.Property(b => b.Status)
               .HasConversion<string>()
               .IsRequired();

        builder.HasOne(b => b.Event)
               .WithMany(e => e.Bookings)
               .HasForeignKey(b => b.EventId);
    }
}
