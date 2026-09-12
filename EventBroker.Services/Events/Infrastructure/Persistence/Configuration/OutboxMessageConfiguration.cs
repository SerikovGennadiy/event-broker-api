using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Events.Application.Common.Messaging; 

namespace Events.Infrastructure.Persistence.Configuration;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        // ПЕРВИЧНЫЙ КЛЮЧ: Наш хронологический UUIDv7
        builder.HasKey(x => x.TraceId);

        // ЗАЩИТА: Говорим EF Core не вмешиваться в генерацию ключа, мы пишем туда готовый UUIDv7
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

        // Тело сообщения (Сырой JSON-контент)
        builder.Property(x => x.Content)
               .HasColumnType("text")
               .IsRequired();

        builder.Property(x => x.Error)
               .HasColumnType("text")
               .IsRequired(false);

        builder.Property(x => x.TimeStampAt)
               .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
               .IsRequired(false);

        // СОСТАВНОЙ ИНДЕКС: Критически важен для OutboxWorker'а!
        // Обеспечивает мгновенный SELECT пачек по 50 штук без полного сканирования таблицы (Table Scan)
        // Запрос: .Where(x => x.ProcessedAtUtc == null).OrderBy(x => x.TimeStampAt).Take(50)
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.TimeStampAt })
               .HasDatabaseName("IX_OutboxMessages_Pending");
    }
}