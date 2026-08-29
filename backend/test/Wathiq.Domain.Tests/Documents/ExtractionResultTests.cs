using System;
using Shouldly;
using Volo.Abp;
using Wathiq.Documents.Extraction;
using Xunit;

namespace Wathiq.Documents;

public class ExtractionResultTests
{
    private static ExtractionResult NewProposed() => new(
        Guid.NewGuid(), attachmentId: Guid.NewGuid(), provider: "ollama", model: "qwen2.5:7b",
        promptVersion: "extract-document@v1", rawJson: "{}", confidence: 0.85m, durationMs: 1200);

    [Fact]
    public void A_Proposed_Result_Concludes_Exactly_Once()
    {
        var result = NewProposed();
        result.Outcome.ShouldBe(ExtractionOutcome.Proposed);

        result.Accept().Outcome.ShouldBe(ExtractionOutcome.Accepted);

        // The verdict is a fact about one review - 3.8's accuracy stats depend on it being final.
        Should.Throw<BusinessException>(() => result.Reject())
            .Code.ShouldBe(DocumentsErrorCodes.ExtractionAlreadyConcluded);
    }

    [Fact]
    public void A_Failed_Row_Never_Becomes_Reviewable()
    {
        var failed = new ExtractionResult(
            Guid.NewGuid(), Guid.NewGuid(), "ollama", "qwen2.5:7b", "extract-document@v1",
            rawJson: "", confidence: null, durationMs: 300, failed: true);

        failed.Outcome.ShouldBe(ExtractionOutcome.Failed);
        Should.Throw<BusinessException>(() => failed.Accept());
    }

    [Fact]
    public void Out_Of_Range_Confidence_Is_Noise_Not_Data()
    {
        new ExtractionResult(Guid.NewGuid(), Guid.NewGuid(), "ollama", "m", "v",
            "{}", confidence: 1.7m, durationMs: 1).Confidence.ShouldBeNull();
        NewProposed().Confidence.ShouldBe(0.85m);
    }
}
