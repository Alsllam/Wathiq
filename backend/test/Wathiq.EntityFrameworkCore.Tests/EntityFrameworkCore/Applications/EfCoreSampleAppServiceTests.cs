using Wathiq.Samples;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Applications;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<WathiqEntityFrameworkCoreTestModule>
{

}
