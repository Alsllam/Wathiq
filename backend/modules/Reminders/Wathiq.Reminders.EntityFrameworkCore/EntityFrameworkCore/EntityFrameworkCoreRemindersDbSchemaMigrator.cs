using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Wathiq.Data;

namespace Wathiq.Reminders.EntityFrameworkCore;

// Third IWathiqDbSchemaMigrator (host, Documents, now Reminders): WathiqDbMigrationService
// iterates them all. Explicit exposure again - the class name does not end with the interface name.
[ExposeServices(typeof(IWathiqDbSchemaMigrator))]
public class EntityFrameworkCoreRemindersDbSchemaMigrator : IWathiqDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreRemindersDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        await _serviceProvider
            .GetRequiredService<RemindersDbContext>()
            .Database
            .MigrateAsync();
    }
}
