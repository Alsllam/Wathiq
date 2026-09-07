using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wathiq.Shared.GuideChat;

namespace Wathiq.Guides;

/// <summary>Scripted model for the chat flow (the FakeDocumentDataExtractor pattern): tests set
/// the next answer, and every call records what the model was shown.</summary>
public class FakeGuideAnswerer : IGuideAnswerer
{
    public string? NextAnswer { get; set; }
    public IReadOnlyList<string> NextCitations { get; set; } = [];
    public List<(string Question, IReadOnlyList<GuideExcerpt> Excerpts)> Calls { get; } = [];

    public Task<GuideModelAnswer> AnswerAsync(
        string question, IReadOnlyList<GuideExcerpt> excerpts, CancellationToken cancellationToken = default)
    {
        Calls.Add((question, excerpts));
        return Task.FromResult(new GuideModelAnswer
        {
            AnswerText = NextAnswer,
            CitedLabels = NextCitations,
            Provider = "fake",
            Model = "fake-chat",
            PromptVersion = "guides-chat@v1",
            DurationMs = 1
        });
    }
}
