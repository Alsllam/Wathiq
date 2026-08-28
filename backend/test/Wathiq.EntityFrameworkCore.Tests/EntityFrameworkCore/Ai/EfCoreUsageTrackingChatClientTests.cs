using Wathiq.Ai;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Ai;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreUsageTrackingChatClientTests : UsageTrackingChatClientTests<WathiqEntityFrameworkCoreTestModule>
{
}
