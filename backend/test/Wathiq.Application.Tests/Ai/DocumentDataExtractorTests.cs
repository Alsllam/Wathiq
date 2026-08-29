using System.Threading.Tasks;
using Shouldly;
using Wathiq.Ai.Extraction;
using Xunit;

namespace Wathiq.Ai;

/* The extractor with the model faked: what reaches the proposal is decided by the PARSERS, not
 * by whatever the model happens to say. Constructed directly (no DI) - the keyed-client wiring
 * itself is certified by the 3.4 smoke; here the seam under test is parse-and-validate. */
public class DocumentDataExtractorTests
{
    private readonly FakeChatClient _chatClient = new();
    private readonly DocumentDataExtractor _extractor;

    public DocumentDataExtractorTests()
    {
        _extractor = new DocumentDataExtractor(_chatClient, new AiOptions());
    }

    [Fact]
    public async Task Clean_Model_Json_Becomes_A_Full_Proposal()
    {
        _chatClient.NextResponseText =
            """{"number":"P-102030","issue_date":"2026-03-01","expiry_date":"2036-03-01","holder_name":"Ahmed Ali","document_kind":"جواز سفر","confidence":0.91}""";

        var proposal = await _extractor.ExtractAsync("PASSPORT No P-102030 ...");

        proposal.Number.ShouldBe("P-102030");
        proposal.IssueDate.ShouldBe(new(2026, 3, 1));
        proposal.ExpiryDate.ShouldBe(new(2036, 3, 1));
        proposal.HolderName.ShouldBe("Ahmed Ali");
        proposal.DocumentKind.ShouldBe("جواز سفر");
        proposal.Confidence.ShouldBe(0.91m);
        proposal.Warnings.ShouldBeEmpty();
        proposal.PromptVersion.ShouldBe(DocumentDataExtractor.PromptVersion);
        proposal.Provider.ShouldBe("ollama");
    }

    [Fact]
    public async Task The_Impossible_Date_Is_Caught_Here()
    {
        // The 3.CP checkpoint scenario, as a living test.
        _chatClient.NextResponseText = """{"number":"A1","expiry_date":"2027-02-30","confidence":0.8}""";

        var proposal = await _extractor.ExtractAsync("...");

        proposal.ExpiryDate.ShouldBeNull();
        proposal.Warnings.ShouldContain(w => w.Contains("2027-02-30"));
        proposal.Number.ShouldBe("A1");   // one bad field never poisons the others
    }

    [Fact]
    public async Task Inverted_Date_Pair_Drops_Both()
    {
        _chatClient.NextResponseText = """{"issue_date":"2030-01-01","expiry_date":"2020-01-01"}""";

        var proposal = await _extractor.ExtractAsync("...");

        proposal.IssueDate.ShouldBeNull();
        proposal.ExpiryDate.ShouldBeNull();
        proposal.Warnings.ShouldHaveSingleItem().ShouldContain("before issue");
    }

    [Fact]
    public async Task Fenced_Output_Still_Parses_And_Garbage_Yields_Only_Warnings()
    {
        _chatClient.NextResponseText = "```json\n{\"number\":\"B77\"}\n```";
        (await _extractor.ExtractAsync("...")).Number.ShouldBe("B77");

        _chatClient.NextResponseText = "Sorry, I cannot help with that.";
        var garbage = await _extractor.ExtractAsync("...");
        garbage.Number.ShouldBeNull();
        garbage.Warnings.ShouldHaveSingleItem().ShouldContain("did not return JSON");
        garbage.RawJson.ShouldContain("Sorry");   // evidence kept for the review UI
    }
}
