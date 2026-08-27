using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wathiq.Reminders.EntityFrameworkCore;

/* Design-time only: lets `dotnet ef migrations add --context RemindersDbContext` build the
 * context without booting the ABP application. Reads the DbMigrator's appsettings like the host factory. */
public class RemindersDbContextFactory : IDesignTimeDbContextFactory<RemindersDbContext>
{
    public RemindersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../../src/Wathiq.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(RemindersDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<RemindersDbContext>()
            .UseSqlServer(connectionString, WathiqRemindersEntityFrameworkCoreModule.ConfigureSqlServer);

        return new RemindersDbContext(builder.Options);
    }
}
