using Wathiq.Samples;
using Xunit;

namespace Wathiq.EntityFrameworkCore.Domains;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<WathiqEntityFrameworkCoreTestModule>
{

}
