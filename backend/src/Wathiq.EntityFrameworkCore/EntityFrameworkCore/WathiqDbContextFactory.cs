using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Wathiq.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class WathiqDbContextFactory : IDesignTimeDbContextFactory<WathiqDbContext>
{
    public WathiqDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        WathiqEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<WathiqDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new WathiqDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Wathiq.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
