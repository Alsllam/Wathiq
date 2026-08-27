using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wathiq.Data;
using Volo.Abp.DependencyInjection;

namespace Wathiq.EntityFrameworkCore;

public class EntityFrameworkCoreWathiqDbSchemaMigrator
    : IWathiqDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreWathiqDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the WathiqDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<WathiqDbContext>()
            .Database
            .MigrateAsync();
    }
}
