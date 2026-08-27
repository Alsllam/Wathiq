using Volo.Abp.Modularity;

namespace Wathiq;

/* Inherit from this class for your domain layer tests. */
public abstract class WathiqDomainTestBase<TStartupModule> : WathiqTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
