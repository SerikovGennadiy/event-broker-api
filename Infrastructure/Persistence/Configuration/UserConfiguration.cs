using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .ValueGeneratedNever();

        // не требовалось
        builder.Property(u => u.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(u => u.Name)
               .IsUnique();

        builder.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(u => u.Role)
               .IsRequired()
               .HasConversion<int>();
    }
}
