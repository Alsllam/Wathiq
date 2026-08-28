using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
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
using Wathiq.Ai.Usage;
using Xunit;

namespace Wathiq.Ai;

/* FR-AI-004 pinned: the decorator caps BEFORE calling and its ledger row outlives a rollback.
 * Concrete class in EFCore.Tests (SQLite). */
public abstract class UsageTrackingChatClientTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<AiUsage, Guid> _usages;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ICurrentPrincipalAccessor _principalAccessor;
    private readonly FakeChatClient _inner = new();

    protected UsageTrackingChatClientTests()
    {
        _usages = GetRequiredService<IRepository<AiUsage, Guid>>();
        _uowManager = GetRequiredService<IUnitOfWorkManager>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private UsageTrackingChatClient NewClient(int cap = 50) => new(
        _inner, AiUsagePurpose.Extraction,
        new AiClientOptions(),                       // ollama / qwen2.5:7b defaults
        new AiOptions { DailyCallCapPerUser = cap },
        GetRequiredService<ICurrentUser>(),
        _usages, _uowManager,
        GetRequiredService<IClock>(),
        GetRequiredService<IGuidGenerator>());

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim(AbpClaimTypes.UserId, userId.ToString())], "test")));

    private static ChatMessage[] Ask() => [new ChatMessage(ChatRole.User, "extract this")];

    [Fact]
    public async Task Every_Call_Lands_In_The_Ledger()
    {
        var userId = Guid.NewGuid();
        var options = new ChatOptions { AdditionalProperties = new AdditionalPropertiesDictionary
            { [AiConsts.PromptVersionOptionKey] = "v1" } };

        using (ActAs(userId))
        {
            await NewClient().GetResponseAsync(Ask(), options);
        }

        var row = (await _usages.GetListAsync(u => u.UserId == userId)).ShouldHaveSingleItem();
        row.Purpose.ShouldBe(AiUsagePurpose.Extraction);
        row.Provider.ShouldBe("ollama");
        row.TokensIn.ShouldBe(120);
        row.TokensOut.ShouldBe(30);
        row.PromptVersion.ShouldBe("v1");   // announced by the caller via ChatOptions
    }

    [Fact]
    public async Task The_Cap_Blocks_Before_The_Model_Is_Reached()
    {
        var userId = Guid.NewGuid();
        var client = NewClient(cap: 2);

        using (ActAs(userId))
        {
            await client.GetResponseAsync(Ask());
            await client.GetResponseAsync(Ask());

            var ex = await Should.ThrowAsync<BusinessException>(() => client.GetResponseAsync(Ask()));
            ex.Code.ShouldBe(AiErrorCodes.DailyCapExceeded);
        }

        _inner.CallCount.ShouldBe(2);   // the third call never left the process
        (await _usages.GetListAsync(u => u.UserId == userId)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task The_Ledger_Survives_A_Rolled_Back_Business_Transaction()
    {
        var userId = Guid.NewGuid();

        using (ActAs(userId))
        {
            using (var uow = _uowManager.Begin(new AbpUnitOfWorkOptions(isTransactional: true)))
            {
                await NewClient().GetResponseAsync(Ask());
                // no CompleteAsync: the ambient transaction rolls back on dispose
            }
        }

        // requiresNew did its job: the call is still on the books (FR-AI-004 is not optional).
        (await _usages.GetListAsync(u => u.UserId == userId)).ShouldHaveSingleItem();
    }
}
