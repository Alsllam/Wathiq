using Wathiq.Ai.Evals;
using Xunit;
using Xunit.Abstractions;

namespace Wathiq.EntityFrameworkCore.Ai;

[Collection(WathiqTestConsts.CollectionDefinitionName)]
public class EfCoreExtractionEvalTests : ExtractionEvalTests<WathiqEntityFrameworkCoreTestModule>
{
    public EfCoreExtractionEvalTests(ITestOutputHelper output)
        : base(output)
    {
    }
}
