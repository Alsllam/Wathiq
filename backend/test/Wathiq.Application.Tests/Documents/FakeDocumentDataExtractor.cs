using System;
using System.Threading;
using System.Threading.Tasks;
using Wathiq.Shared.Extraction;

namespace Wathiq.Documents;

/// <summary>
/// The AI seam, scripted. The real DocumentDataExtractor (Ai module) would dial Ollama; here
/// the flow under test is Documents' escrow-and-confirm, so the model is a puppet: a fixed
/// proposal, or a thrown exception to exercise the Failed path.
/// </summary>
public class FakeDocumentDataExtractor : IDocumentDataExtractor
{
    public DocumentDataProposal NextProposal { get; set; } = NewProposal();
    public Exception? NextException { get; set; }
    public string? LastOcrText { get; private set; }

    public static DocumentDataProposal NewProposal() => new()
    {
        Number = "P-102030",
        IssueDate = new DateOnly(2026, 3, 1),
        ExpiryDate = new DateOnly(2036, 3, 1),
        HolderName = "Ahmed Ali",
        DocumentKind = "جواز سفر",
        Confidence = 0.9m,
        RawJson = """{"number":"P-102030"}""",
        PromptVersion = "extract-document@v1",
        Provider = "ollama",
        Model = "qwen2.5:7b",
        DurationMs = 1500
    };

    public Task<DocumentDataProposal> ExtractAsync(string ocrText, CancellationToken cancellationToken = default)
    {
        LastOcrText = ocrText;
        return NextException != null ? Task.FromException<DocumentDataProposal>(NextException) : Task.FromResult(NextProposal);
    }

    // Deterministic like the real one: same stored json -> same proposal.
    public DocumentDataProposal ParseStored(string rawJson) => NextProposal;
}
