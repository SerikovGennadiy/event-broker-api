using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Users.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        //  Передаем РЕАЛЬНОЕ имя папки, где лежит ваш appsettings.json со времен монолита
        var configuration = GetConfigurationFromProject("Presentation");

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: m => m.MigrationsAssembly("Users.Infrastructure"));

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

                // Ищем папку "Users", внутри которой лежат слои Infrastructure, Presentation и т.д.
                if (dirInfo.Name.Equals("Users", StringComparison.OrdinalIgnoreCase))
                {
                    return currentDir;
                }

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            // Если папка "Users" не найдена, возвращаем базовый путь (страховка для Docker)
            return AppContext.BaseDirectory;
        }
    }
}
