using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wathiq.Ai.EntityFrameworkCore;

/* Design-time only - same shape as the Documents/Reminders factories. */
public class AiDbContextFactory : IDesignTimeDbContextFactory<AiDbContext>
{
    public AiDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../../src/Wathiq.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(AiDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<AiDbContext>()
            .UseSqlServer(connectionString, WathiqAiEntityFrameworkCoreModule.ConfigureSqlServer);

        return new AiDbContext(builder.Options);
    }
}
