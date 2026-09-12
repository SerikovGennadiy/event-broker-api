using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Events.Application.Common.Messaging;

namespace Events.Infrastructure.Persistence.Configuration;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        // Имя таблицы в PostgreSQL (желательно в snake_case или PascalCase, как принято в проекте)
        builder.ToTable("InboxMessages");

        // ПЕРВИЧНЫЙ КЛЮЧ: Сквозной TraceId сообщения
        builder.HasKey(x => x.TraceId);

        // ЗАЩИТА: Отключаем автоматическую генерацию ID базой данных, так как ключ всегда пишется руками
        builder.Property(x => x.TraceId)
               .ValueGeneratedNever()
               .IsRequired();

        builder.Property(x => x.Type)
               .HasMaxLength(255)
               .IsRequired();

        // Поле Content может быть null при штатной работе, тип text в Postgres идеален для JSON строк
        // Content - заполняется только при ошибке!
        builder.Property(x => x.Content)
               .HasColumnType("text")
               .IsRequired(false);

        builder.Property(x => x.Error)
               .HasColumnType("text")
               .IsRequired(false);

        builder.Property(x => x.ReadAttempts)
               .HasDefaultValue(0)
               .IsRequired();

        builder.Property(x => x.ReceivedAtUtc)
               .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
               .IsRequired(false);

        // ИНДЕКС: Обеспечивает мгновенную фильтрацию дубликатов на Шаге 1 консьюмера
        // Запрос: .AnyAsync(m => m.TraceId == id && m.ProcessedAtUtc != null)
        builder.HasIndex(x => new { x.TraceId, x.ProcessedAtUtc })
               .HasDatabaseName("IX_InboxMessages_Verification");
    }
}