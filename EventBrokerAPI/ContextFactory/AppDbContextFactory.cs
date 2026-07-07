using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;

namespace EventBrokerAPI.ContextFactory;
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                      .AddJsonFile("appsettings.json")
                                                      .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString: configuration.GetConnectionString("DefaultConnection"),
                       npgsqlOptionsAction: m => m.MigrationsAssembly("EventBrokerAPI"));

        return new AppDbContext(builder.Options);
    }
}
