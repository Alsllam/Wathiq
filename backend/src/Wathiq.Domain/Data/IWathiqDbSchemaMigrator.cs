using System.Threading.Tasks;

namespace Wathiq.Data;

public interface IWathiqDbSchemaMigrator
{
    Task MigrateAsync();
}
