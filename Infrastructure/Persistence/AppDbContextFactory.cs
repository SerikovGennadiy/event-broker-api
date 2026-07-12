using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var solutionPath = GetSolutionDirectory();

        var configuration = new ConfigurationBuilder().SetBasePath(Path.Combine(solutionPath, "EventBrokerAPI"))
                                                      .AddJsonFile("appsettings.json")
                                                      .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString: configuration.GetConnectionString("DefaultConnection"),
                       npgsqlOptionsAction: m => m.MigrationsAssembly("Infrastructure"));

        return new AppDbContext(builder.Options);
    }

    private static string GetSolutionDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Directory.GetFiles(currentDir, "*.sln").Any())
                return currentDir;

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new FileNotFoundException("Solution file (.sln) not found");
    }
}
