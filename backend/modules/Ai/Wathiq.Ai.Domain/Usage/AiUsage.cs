using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Wathiq.Ai.Usage;

/// <summary>
/// One AI call, as accounting (FR-AI-004). An append-only ledger line: constructor-only state,
/// no setters, no soft delete - which is why this is a Basic aggregate, not FullAudited (DB4
/// audit columns would be dead weight on rows that are never edited or deleted).
/// </summary>
public class AiUsage : BasicAggregateRoot<Guid>
{
    public Guid? UserId { get; private set; }        // null = system job (eval runs, re-embedding)
    public AiUsagePurpose Purpose { get; private set; }
    public string Provider { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public string? PromptVersion { get; private set; }
    public int TokensIn { get; private set; }
    public int TokensOut { get; private set; }
    public int DurationMs { get; private set; }
    public DateTime At { get; private set; }         // UTC (DB6)

    private AiUsage()
    {
    }

    public AiUsage(
        Guid id, Guid? userId, AiUsagePurpose purpose, string provider, string model,
        DateTime atUtc, int tokensIn, int tokensOut, int durationMs, string? promptVersion = null)
        : base(id)
    {
        UserId = userId;
        Purpose = purpose;
        Provider = Check.NotNullOrWhiteSpace(provider, nameof(provider), AiUsageConsts.MaxProviderLength);
        Model = Check.NotNullOrWhiteSpace(model, nameof(model), AiUsageConsts.MaxModelLength);
        PromptVersion = promptVersion.IsNullOrWhiteSpace()
            ? null
            : Check.Length(promptVersion, nameof(promptVersion), AiUsageConsts.MaxPromptVersionLength);
        TokensIn = tokensIn;
        TokensOut = tokensOut;
        DurationMs = durationMs;
        At = atUtc;
    }
}
