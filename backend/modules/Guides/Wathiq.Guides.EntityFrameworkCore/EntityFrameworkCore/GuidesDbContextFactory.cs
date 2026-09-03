using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wathiq.Guides.EntityFrameworkCore;

/* Design-time only - same shape as the Documents/Reminders/Ai factories. */
public class GuidesDbContextFactory : IDesignTimeDbContextFactory<GuidesDbContext>
{
    public GuidesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../../src/Wathiq.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(GuidesDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<GuidesDbContext>()
            .UseSqlServer(connectionString, WathiqGuidesEntityFrameworkCoreModule.ConfigureSqlServer);

        return new GuidesDbContext(builder.Options);
    }
}
