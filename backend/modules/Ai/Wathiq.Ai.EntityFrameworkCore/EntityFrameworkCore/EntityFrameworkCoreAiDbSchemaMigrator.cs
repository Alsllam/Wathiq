using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Wathiq.Data;

namespace Wathiq.Ai.EntityFrameworkCore;

// Fourth IWathiqDbSchemaMigrator; explicit exposure as always (class name doesn't end with the interface name).
[ExposeServices(typeof(IWathiqDbSchemaMigrator))]
public class EntityFrameworkCoreAiDbSchemaMigrator : IWathiqDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAiDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        await _serviceProvider
            .GetRequiredService<AiDbContext>()
            .Database
            .MigrateAsync();
    }
}
