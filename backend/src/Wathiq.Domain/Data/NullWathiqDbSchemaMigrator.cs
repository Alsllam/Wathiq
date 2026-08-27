using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Wathiq.Data;

/* This is used if database provider does't define
 * IWathiqDbSchemaMigrator implementation.
 */
public class NullWathiqDbSchemaMigrator : IWathiqDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
