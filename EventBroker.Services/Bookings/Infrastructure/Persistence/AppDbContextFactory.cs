using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bookings.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Передаем РЕАЛЬНОЕ имя папки, где лежит ваш appsettings.json со времен монолита
        var configuration = GetConfigurationFromProject("Presentation");

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: m => m.MigrationsAssembly("Bookings.Infrastructure"));

        return new AppDbContext(builder.Options);
    }

    public static IConfigurationRoot GetConfigurationFromProject(string apiFolderName)
    {
        // 1. Пытаемся прочитать переменную окружения (Docker/Production стандарт)
        var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // 2. Если мы запускаем локально на машине разработчика (Design-time CLI)
        var microserviceDirectory = FindMicroserviceDirectory();

        // Cтроим путь к реальной папке "Presentation"
        var projectPath = Path.Combine(microserviceDirectory, apiFolderName);
        // Если папка Presentation не найдена (например, мы уже внутри Docker-контейнера без .sln)
        if (!Directory.Exists(projectPath))
        {
            // Подстраховка: берем appsettings.json из текущей директории запуска
            projectPath = AppContext.BaseDirectory;
        }

        return new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        string FindMicroserviceDirectory()
        {
            // Начинаем оттуда, где запущена фабрика (например, bin/Debug/net9.0)
            var currentDir = AppContext.BaseDirectory;

            while (currentDir != null)
            {
                var dirInfo = new DirectoryInfo(currentDir);

                // Ищем папку "Bookings", внутри которой лежат слои Infrastructure, Presentation и т.д.
                if (dirInfo.Name.Equals("Bookings", StringComparison.OrdinalIgnoreCase))
                {
                    return currentDir;
                }

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            // Если папка "Bookings" не найдена, возвращаем базовый путь (страховка для Docker)
            return AppContext.BaseDirectory;
        }
    }
}

#region Hint
// путь до проекта с application.json (Bookings.API) физически отличается. 
//dotnet ef migrations add InitialDatabase --project../Infrastructure/Bookings.Infrastructure.csproj --startup-project../Presentation/Bookings.API.csproj
// -- project - проект где держать миграции (появится папка Migrations) --startup-project - запуск  
#endregion