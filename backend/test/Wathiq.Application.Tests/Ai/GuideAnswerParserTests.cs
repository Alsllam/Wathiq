using Shouldly;
using Wathiq.Ai.GuideChat;
using Xunit;

namespace Wathiq.Ai;

/* The RAG twin of ExtractedValueParserTests: every malformed model output degrades to a
 * refusal (null answer), never to an exception or an invented answer. */
public class GuideAnswerParserTests
{
    [Fact]
    public void Parses_The_Happy_Shape_With_Fences()
    {
        var (answer, citations) = GuideAnswerParser.Parse(
            "```json\n{\"answer\": \"الرسوم 300 ريال.\", \"citations\": [\"C1\", \"C3\"]}\n```");

        answer.ShouldBe("الرسوم 300 ريال.");
        citations.ShouldBe(["C1", "C3"]);
    }

    [Fact]
    public void Null_Or_Whitespace_Answer_Is_A_Refusal()
    {
        GuideAnswerParser.Parse("{\"answer\": null, \"citations\": []}").Answer.ShouldBeNull();
        GuideAnswerParser.Parse("{\"answer\": \"  \", \"citations\": [\"C1\"]}").Answer.ShouldBeNull();
    }

    [Fact]
    public void Garbage_And_Torn_Json_Refuse_Instead_Of_Throwing()
    {
        GuideAnswerParser.Parse("I think the fee is 300 SAR.").Answer.ShouldBeNull();   // prose, no JSON
        GuideAnswerParser.Parse("{\"answer\": \"x\", \"citations\": [\"C1\"").Answer.ShouldBeNull();   // torn
    }

    [Fact]
    public void Citation_Noise_Is_Skipped_Not_Fatal()
    {
        var (answer, citations) = GuideAnswerParser.Parse(
            "{\"answer\": \"ok\", \"citations\": [\"C1\", 7, null, \"\", \"C2\"]}");

        answer.ShouldBe("ok");
        citations.ShouldBe(["C1", "C2"]);   // numbers, nulls, empties dropped silently
    }

    [Fact]
    public void Missing_Citations_Field_Means_No_Citations()
    {
        var (answer, citations) = GuideAnswerParser.Parse("{\"answer\": \"ok\"}");

        answer.ShouldBe("ok");
        citations.ShouldBeEmpty();   // the SERVICE decides that an uncited answer is refused
    }
}
