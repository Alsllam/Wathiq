using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wathiq.Documents.EntityFrameworkCore;

/* Design-time only: lets `dotnet ef migrations add --context DocumentsDbContext` build the
 * context without booting the ABP application. Reads the DbMigrator's appsettings like the host factory. */
public class DocumentsDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../../src/Wathiq.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(DocumentsDbProperties.ConnectionStringName)
                               ?? configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlServer(connectionString, WathiqDocumentsEntityFrameworkCoreModule.ConfigureSqlServer);

        return new DocumentsDbContext(builder.Options);
    }
}
