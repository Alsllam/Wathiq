using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Documents.Extraction;

/// <summary>
/// One AI extraction attempt for one attachment (database.md E-ExtractionResult). Append-only
/// like ai.Usage - a re-extraction is a NEW row, so 3.8 can measure prompt versions against each
/// other. The only mutable thing is the human verdict: Proposed is the single fork point, and
/// each terminal outcome (Accepted/Edited/Rejected) is final. Rows born Failed stay Failed.
/// </summary>
public class ExtractionResult : CreationAuditedAggregateRoot<Guid>
{
    public Guid AttachmentId { get; private set; }
    public string Provider { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public string PromptVersion { get; private set; } = default!;
    /// <summary>The model's JSON as validated by the Ai module - PII, purged 90 days after acceptance (P8).</summary>
    public string RawJson { get; private set; } = default!;
    public decimal? Confidence { get; private set; }
    public ExtractionOutcome Outcome { get; private set; }
    public int DurationMs { get; private set; }

    private ExtractionResult()
    {
    }

    public ExtractionResult(
        Guid id, Guid attachmentId, string provider, string model, string promptVersion,
        string rawJson, decimal? confidence, int durationMs, bool failed = false)
        : base(id)
    {
        AttachmentId = attachmentId;
        Provider = Check.NotNullOrWhiteSpace(provider, nameof(provider), ExtractionResultConsts.MaxProviderLength);
        Model = Check.NotNullOrWhiteSpace(model, nameof(model), ExtractionResultConsts.MaxModelLength);
        PromptVersion = Check.NotNullOrWhiteSpace(promptVersion, nameof(promptVersion), ExtractionResultConsts.MaxPromptVersionLength);
        RawJson = Check.NotNull(rawJson, nameof(rawJson));
        Confidence = confidence is >= 0m and <= 1m ? confidence : null;   // out-of-range is noise, not data
        DurationMs = durationMs;
        Outcome = failed ? ExtractionOutcome.Failed : ExtractionOutcome.Proposed;
    }

    public ExtractionResult Accept() => Conclude(ExtractionOutcome.Accepted);
    public ExtractionResult MarkEdited() => Conclude(ExtractionOutcome.Edited);
    public ExtractionResult Reject() => Conclude(ExtractionOutcome.Rejected);

    private ExtractionResult Conclude(ExtractionOutcome verdict)
    {
        if (Outcome != ExtractionOutcome.Proposed)
        {
            // A verdict is a fact about ONE review; changing it would corrupt 3.8's accuracy stats.
            throw new BusinessException(DocumentsErrorCodes.ExtractionAlreadyConcluded)
                .WithData("Outcome", Outcome.ToString());
        }

        Outcome = verdict;
        return this;
    }
}
