using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Events.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Передаем точное имя папки API-проекта
        var configuration = GetConfigurationFromProject("Events.API");

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: m => m.MigrationsAssembly("Bookings.Infrastructure")); // Указываем полное имя сборки миграций

        return new AppDbContext(builder.Options);
    }

    /// <summary>Получает конфигурацию из appsettings.json указанного проекта решения.</summary>
    /// <param name="apiProjectName">Название папки проекта API (например, "Bookings.API")</param>
    public static IConfigurationRoot GetConfigurationFromProject(string apiProjectName)
    {
        if (string.IsNullOrWhiteSpace(apiProjectName))
            throw new ArgumentException("Название проекта не может быть пустым.", nameof(apiProjectName));

        var solutionDirectory = FindSolutionDirectory();

        // Строим путь согласно структуре: sln -> Services -> Bookings -> Bookings.API
        var projectPath = Path.Combine(solutionDirectory, "EventBroker.Services", "Bookings", apiProjectName);

        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException($"Папка проекта '{apiProjectName}' не найдена по пути '{projectPath}'");

        var appsettingsPath = Path.Combine(projectPath, "appsettings.json");

        if (!File.Exists(appsettingsPath))
            throw new FileNotFoundException($"Файл 'appsettings.json' не найден в проекте '{apiProjectName}'");

        return new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        string FindSolutionDirectory()
        {
            // При выполнении миграций CurrentDirectory может быть папкой проекта Infrastructure.
            // Нам нужно подняться вверх до файла .sln
            var currentDir = AppContext.BaseDirectory;

            while (currentDir != null)
            {
                if (Directory.GetFiles(currentDir, "*.sln").Any())
                    return currentDir;

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            throw new FileNotFoundException("Файл решения (.sln) не найден при поиске вверх от папки выполнения.");
        }
    }
}
