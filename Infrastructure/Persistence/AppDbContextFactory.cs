using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = GetConfigurationFromProject("Presentation");

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: m => m.MigrationsAssembly("Infrastructure"));

        return new AppDbContext(builder.Options);
    }

    /// <summary>Получает конфигурацию из appsettings.json указанного проекта решения.</summary>
    /// <param name="projectName">Название папки проекта внутри решения</param>
    /// <returns>Конфигурация из appsettings.json проекта</returns>
    /// <exception cref="FileNotFoundException">Если файл решения (.sln) или appsettings.json не найдены</exception>
    /// <exception cref="DirectoryNotFoundException">Если папка проекта не найдена</exception>
    public static IConfigurationRoot GetConfigurationFromProject(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Название проекта не может быть пустым.", nameof(projectName));

        var solutionDirectory = FindSolutionDirectory();
        var projectPath = Path.Combine(solutionDirectory, projectName);

        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException($"Папка проекта '{projectName}' не найдена по пути '{projectPath}'");

        var appsettingsPath = Path.Combine(projectPath, "appsettings.json");

        if (!File.Exists(appsettingsPath))
            throw new FileNotFoundException($"Файл 'appsettings.json' не найден в проекте '{projectName}'");

        return new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        string FindSolutionDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();

            while (currentDir != null)
            {
                if (Directory.GetFiles(currentDir, "*.sln").Any())
                    return currentDir;

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            throw new FileNotFoundException("Файл решения (.sln) не найден");
        }
    }
}
