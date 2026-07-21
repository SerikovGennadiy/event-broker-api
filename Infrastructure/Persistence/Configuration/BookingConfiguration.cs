using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Models;

namespace Infrastructure.Persistence.Configuration;

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

        builder.Property(b => b.UserId)
               .IsRequired();

        builder.Property(b => b.CreatedAt)
               .IsRequired();

        builder.Property(b => b.Status)
               .IsRequired()
               .HasConversion<int>();   

        builder.HasOne(b => b.Event)
               .WithMany()
               .HasForeignKey(b => b.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.User)
               .WithMany()
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
