using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Wathiq.Data;

namespace Wathiq.Documents.EntityFrameworkCore;

// WathiqDbMigrationService iterates IEnumerable<IWathiqDbSchemaMigrator>, so a second
// implementation is all `DbMigrator` needs to migrate this context too. ABP's convention only
// exposes an interface whose name the class name ends with (…WathiqDbSchemaMigrator); this class
// does not, so the exposure must be explicit or the migrator silently never runs.
[ExposeServices(typeof(IWathiqDbSchemaMigrator))]
public class EntityFrameworkCoreDocumentsDbSchemaMigrator : IWathiqDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreDocumentsDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        await _serviceProvider
            .GetRequiredService<DocumentsDbContext>()
            .Database
            .MigrateAsync();
    }
}
