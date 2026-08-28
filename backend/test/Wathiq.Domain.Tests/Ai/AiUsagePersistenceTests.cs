using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Ai.Usage;
using Xunit;

namespace Wathiq.Ai;

/* The ledger round-trips: enum-as-byte, lengths, and the nullable UserId (system rows).
 * Concrete class in EFCore.Tests (SQLite). */
public abstract class AiUsagePersistenceTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IRepository<AiUsage, Guid> _usages;

    protected AiUsagePersistenceTests()
    {
        _usages = GetRequiredService<IRepository<AiUsage, Guid>>();
    }

    [Fact]
    public async Task Ledger_Rows_Round_Trip_For_Users_And_System_Jobs()
    {
        var userId = Guid.NewGuid();
        var at = new DateTime(2026, 8, 28, 12, 0, 0, DateTimeKind.Utc);

        await WithUnitOfWorkAsync(async () =>
        {
            await _usages.InsertAsync(new AiUsage(
                Guid.NewGuid(), userId, AiUsagePurpose.Extraction, "ollama", "qwen2.5:7b",
                at, tokensIn: 1200, tokensOut: 250, durationMs: 3400, promptVersion: "v1"));
            await _usages.InsertAsync(new AiUsage(
                Guid.NewGuid(), userId: null, AiUsagePurpose.Eval, "ollama", "qwen2.5:7b",
                at, tokensIn: 90, tokensOut: 20, durationMs: 800));
        });

        var mine = await _usages.GetListAsync(u => u.UserId == userId);
        var row = mine.ShouldHaveSingleItem();
        row.Purpose.ShouldBe(AiUsagePurpose.Extraction);
        row.PromptVersion.ShouldBe("v1");
        row.At.ShouldBe(at);

        // System rows (null user) exist but never count toward anyone's daily cap.
        (await _usages.GetListAsync(u => u.UserId == null)).ShouldHaveSingleItem()
            .Purpose.ShouldBe(AiUsagePurpose.Eval);
    }
}
