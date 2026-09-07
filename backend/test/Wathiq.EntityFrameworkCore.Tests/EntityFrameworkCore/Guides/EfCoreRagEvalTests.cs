using Wathiq.Guides.Evals;
using Xunit;
using Xunit.Abstractions;

namespace Wathiq.EntityFrameworkCore.Guides;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreRagEvalTests : RagEvalTests<WathiqEntityFrameworkCoreTestModule>
{
    public EfCoreRagEvalTests(ITestOutputHelper output) : base(output)
    {
    }
}
