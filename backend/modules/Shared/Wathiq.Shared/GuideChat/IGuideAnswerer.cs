using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wathiq.Shared.GuideChat;

/// <summary>One retrieved excerpt as the model sees it: an opaque label plus text. The label→chunk
/// mapping stays on the Guides side - the model never learns real ids, so it cannot leak them.</summary>
public record GuideExcerpt(string Label, string Text);

/// <summary>The model's answer, parsed but NOT validated: labels are whatever the model claimed.
/// Null AnswerText means the model declined (or returned garbage - same safe outcome).</summary>
public class GuideModelAnswer
{
    public string? AnswerText { get; set; }
    public IReadOnlyList<string> CitedLabels { get; set; } = [];
    public string Provider { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string PromptVersion { get; set; } = default!;
    public int DurationMs { get; set; }
}

/// <summary>
/// The Guides→Ai seam (the IDocumentDataExtractor pattern for RAG): Guides hands over the
/// question and its retrieved excerpts, gets a parsed answer back, and never learns which model
/// or prompt produced it. The implementation rides the keyed "guides" client, so the FR-AI-004
/// cap/ledger decorator (purpose GuideChat) is structurally in every call's path.
/// </summary>
public interface IGuideAnswerer
{
    Task<GuideModelAnswer> AnswerAsync(
        string question, IReadOnlyList<GuideExcerpt> excerpts, CancellationToken cancellationToken = default);
}
