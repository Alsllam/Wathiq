using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Wathiq.Ai.Usage;

namespace Wathiq.Ai.Clients;

/// <summary>
/// The FR-AI-004 middleware: an HTTP-interceptor for model calls. Before the call leaves the
/// process it enforces the per-user daily cap; after it returns, one AiUsage ledger row is
/// written in its OWN unit of work (requiresNew) so accounting survives even when the business
/// transaction that triggered the call rolls back.
/// </summary>
public class UsageTrackingChatClient : DelegatingChatClient
{
    private readonly AiUsagePurpose _purpose;
    private readonly AiClientOptions _client;
    private readonly AiOptions _options;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AiUsage, Guid> _usages;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public UsageTrackingChatClient(
        IChatClient inner,
        AiUsagePurpose purpose,
        AiClientOptions client,
        AiOptions options,
        ICurrentUser currentUser,
        IRepository<AiUsage, Guid> usages,
        IUnitOfWorkManager uowManager,
        IClock clock,
        IGuidGenerator guidGenerator)
        : base(inner)
    {
        _purpose = purpose;
        _client = client;
        _options = options;
        _currentUser = currentUser;
        _usages = usages;
        _uowManager = uowManager;
        _clock = clock;
        _guidGenerator = guidGenerator;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        await EnsureUnderCapAsync(userId);

        var stopwatch = Stopwatch.StartNew();
        var response = await base.GetResponseAsync(messages, options, cancellationToken);
        stopwatch.Stop();

        await WriteLedgerAsync(userId, options,
            (int)(response.Usage?.InputTokenCount ?? 0),
            (int)(response.Usage?.OutputTokenCount ?? 0),
            (int)stopwatch.ElapsedMilliseconds);

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Streaming is capped and logged too (token counts refined when a streaming consumer exists, P5).
        var userId = _currentUser.Id;
        await EnsureUnderCapAsync(userId);

        var stopwatch = Stopwatch.StartNew();
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            yield return update;
        }

        stopwatch.Stop();
        await WriteLedgerAsync(userId, options, 0, 0, (int)stopwatch.ElapsedMilliseconds);
    }

    private async Task EnsureUnderCapAsync(Guid? userId)
    {
        if (userId == null)
        {
            return;   // system jobs (evals, re-embedding) are logged but not capped
        }

        // Own UoW: this client runs on any thread (HTTP, Hangfire, tests). CountAsync(predicate)
        // is an EXTENSION over GetQueryableAsync(), and without an ambient UoW the queryable's
        // DbContext is disposed before the query runs - so the check brings its own.
        using var uow = _uowManager.Begin(new AbpUnitOfWorkOptions());

        // IX_Usage_UserId_At exists for exactly this count. UTC day boundary, all purposes.
        var todayUtc = _clock.Now.Date;
        var callsToday = await _usages.CountAsync(u => u.UserId == userId && u.At >= todayUtc);
        await uow.CompleteAsync();

        if (callsToday >= _options.DailyCallCapPerUser)
        {
            throw new BusinessException(AiErrorCodes.DailyCapExceeded)
                .WithData("Cap", _options.DailyCallCapPerUser);
        }
    }

    private async Task WriteLedgerAsync(Guid? userId, ChatOptions? options, int tokensIn, int tokensOut, int durationMs)
    {
        // The caller (3.6's extractor) can announce its prompt version without this layer knowing prompts.
        string? promptVersion = null;
        if (options?.AdditionalProperties?.TryGetValue(AiConsts.PromptVersionOptionKey, out object? v) == true)
        {
            promptVersion = v?.ToString();
        }

        // requiresNew: the ledger commits regardless of what happens to the ambient transaction.
        using var uow = _uowManager.Begin(new AbpUnitOfWorkOptions(), requiresNew: true);
        await _usages.InsertAsync(new AiUsage(
            _guidGenerator.Create(), userId, _purpose, _client.Provider, _client.Model,
            _clock.Now, tokensIn, tokensOut, durationMs, promptVersion));
        await uow.CompleteAsync();
    }
}
