using Volo.Abp.Modularity;

namespace Wathiq;

public abstract class WathiqApplicationTestBase<TStartupModule> : WathiqTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
