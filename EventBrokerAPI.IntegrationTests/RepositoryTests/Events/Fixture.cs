using Microsoft.EntityFrameworkCore;
using Npgsql;
using Repository;
using Testcontainers.PostgreSql;

namespace EventBrokerAPI.IntegrationTests.RepositoryTests.Events;

public class Fixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString: _postgres.GetConnectionString(),
                       npgsqlOptionsAction: m => m.MigrationsAssembly("EventBrokerAPI"))
            .Options;

        return new AppDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        // 1. Критически важно: сбрасываем пул соединений ПЕРЕД любыми операциями с БД
        NpgsqlConnection.ClearAllPools();

        await using var context = CreateTestDbContext();

        await context.Database.MigrateAsync();

        // 3. Получаем имена таблиц ИЗ МОДЕЛИ EF Core (это гарантирует соответствие имен в коде и в БД)
        // Используем обычный ToList(), так как GetEntityTypes() работает с памятью, а не с БД.
        var tableNames = context.Model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .ToList();

        if (!tableNames.Any())
        {
            // Если в модели вообще нет сущностей, сбрасывать нечего
            return;
        }

        const string migrationsHistoryTable = "__EFMigrationsHistory";

        var tablesToTruncate = tableNames
            .Where(t => t != migrationsHistoryTable)
            .ToList();

        if (!tablesToTruncate.Any())
            return;

        // 4. Формируем SQL. Кавычки обязательны для PostgreSQL, чтобы сохранить регистр букв.
        // Если таблица называется "Bookings", без кавычек PG будет искать "bookings".
        var quotedTables = string.Join(", ", tablesToTruncate.Select(t => $"\"{t}\""));
        var sql = $"TRUNCATE TABLE {quotedTables} RESTART IDENTITY CASCADE;";

        // 5. Выполняем очистку.
        // Если таблиц всё ещё нет (например, миграция упала с ошибкой, но не выбросила исключение явно),
        // этот шаг упадет. Поэтому лучше добавить проверку или обработку.
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
        {
            throw new InvalidOperationException(
                $"Ошибка при очистке БД: одна или несколько таблиц не найдены. " +
                $"Проверьте, успешно ли применились миграции. Отсутствующая таблица может быть частью ошибки. " +
                $"Список таблиц из модели: [{string.Join(", ", tablesToTruncate)}]", ex);
        }
    }
}
