using Wathiq.Shared;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Shared;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreWathiqSharedLocalizationTests : WathiqSharedLocalizationTests<WathiqEntityFrameworkCoreTestModule>
{
}
