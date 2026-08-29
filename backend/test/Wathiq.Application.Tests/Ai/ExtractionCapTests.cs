using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Wathiq.Ai.Clients;
using Wathiq.Ai.Extraction;
using Wathiq.Ai.Usage;
using Xunit;

namespace Wathiq.Ai;

/* FR-AI-004 through the REAL composition (3.8): extractor -> cap/ledger decorator -> model,
 * with only the model faked. 3.3 proved the decorator alone; this proves the extractor actually
 * sits behind it and that its prompt version reaches the ledger. Concrete class in EFCore.Tests. */
public abstract class ExtractionCapTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<AiUsage, Guid> _usages;
    private readonly ICurrentPrincipalAccessor _principalAccessor;
    private readonly FakeChatClient _model = new();

    protected ExtractionCapTests()
    {
        _usages = GetRequiredService<IRepository<AiUsage, Guid>>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private DocumentDataExtractor NewExtractor(int cap)
    {
        var options = new AiOptions { DailyCallCapPerUser = cap };
        // The exact onion the host builds in 3.3's RegisterChatClient - minus the socket.
        var capped = new UsageTrackingChatClient(
            _model, AiUsagePurpose.Extraction, options.Extraction, options,
            GetRequiredService<ICurrentUser>(), _usages,
            GetRequiredService<IUnitOfWorkManager>(),
            GetRequiredService<IClock>(), GetRequiredService<IGuidGenerator>());
        return new DocumentDataExtractor(capped, options);
    }

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim(AbpClaimTypes.UserId, userId.ToString())], "test")));

    [Fact]
    public async Task The_Cap_Stops_Extraction_Before_The_Model_And_The_Ledger_Names_The_Prompt()
    {
        var userId = Guid.NewGuid();
        var extractor = NewExtractor(cap: 1);
        _model.NextResponseText = """{"number":"P-1","confidence":0.9}""";

        using (ActAs(userId))
        {
            (await extractor.ExtractAsync("PASSPORT P-1")).Number.ShouldBe("P-1");

            var blocked = await Should.ThrowAsync<BusinessException>(() => extractor.ExtractAsync("PASSPORT P-2"));
            blocked.Code.ShouldBe(AiErrorCodes.DailyCapExceeded);
        }

        _model.CallCount.ShouldBe(1);   // the second call never reached the model

        // One ledger row, stamped with the extractor's own prompt version (FR-AI-004 + FR-AI-005).
        var row = (await _usages.GetListAsync(u => u.UserId == userId)).ShouldHaveSingleItem();
        row.PromptVersion.ShouldBe(DocumentDataExtractor.PromptVersion);
        row.Purpose.ShouldBe(AiUsagePurpose.Extraction);
    }
}
