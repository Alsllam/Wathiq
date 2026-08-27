using Xunit;

namespace Wathiq.EntityFrameworkCore;

[CollectionDefinition(WathiqTestConsts.CollectionDefinitionName)]
public class WathiqEntityFrameworkCoreCollection : ICollectionFixture<WathiqEntityFrameworkCoreFixture>
{

}
