using Wathiq.Guides;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Guides;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreGuideChatAppServiceTests : GuideChatAppServiceTests<WathiqEntityFrameworkCoreTestModule>
{
}
