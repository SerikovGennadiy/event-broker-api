using Npgsql;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Repository;

public static class MigrationManager
{
    private static int _numbersOfRetries;

    public static IHost MigrateDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            using var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                appContext.Database.Migrate();
            }
            catch (PostgresException)
            {
                if (_numbersOfRetries < 6)
                {
                    Thread.Sleep(10000);

                    _numbersOfRetries++;

                    Console.WriteLine($"Сервер СУД не найден или недоступен. Повторная попытка....#{_numbersOfRetries}");

                    MigrateDatabase(host);
                }
                throw;
            }
        }

        return host;
    }
}
