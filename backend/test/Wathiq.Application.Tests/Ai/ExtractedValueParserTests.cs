using Shouldly;
using Wathiq.Ai.Extraction;
using Xunit;

namespace Wathiq.Ai;

/* The FR-AI-003 defense layer as pure functions - including the phase checkpoint's own case. */
public class ExtractedValueParserTests
{
    [Theory]
    [InlineData("2027-05-14", true)]
    [InlineData("2027-02-30", false)]   // the checkpoint question: impossible calendar date
    [InlineData("2027-13-01", false)]   // month 13
    [InlineData("14/05/2027", false)]   // wrong format is wrong, even if a human could read it
    [InlineData("٢٠٢٧-٠٥-١٤", true)]    // Arabic-Indic digits normalize, then parse strictly
    [InlineData("tomorrow", false)]
    [InlineData(null, false)]
    public void Dates_Survive_Only_As_Real_Iso_Days(string? input, bool expected)
    {
        (ExtractedValueParser.TryParseDate(input) != null).ShouldBe(expected);
    }

    [Theory]
    [InlineData("P-102030", "P-102030")]
    [InlineData("  ٤٤١٢٣٤٥٦٧  ", "441234567")]              // trimmed + digits normalized
    [InlineData("DROP TABLE documents;--", null)]            // allow-list says no
    [InlineData("<script>alert(1)</script>", null)]
    [InlineData("", null)]
    public void Numbers_Pass_The_Allow_List_Or_Vanish(string input, string? expected)
    {
        ExtractedValueParser.TryParseDocumentNumber(input).ShouldBe(expected);
    }

    [Fact]
    public void Fenced_Model_Output_Yields_The_Inner_Object()
    {
        ExtractedValueParser.ExtractJsonObject("```json\n{\"a\":1}\n```").ShouldBe("{\"a\":1}");
        ExtractedValueParser.ExtractJsonObject("no json here").ShouldBeNull();
    }

    [Fact]
    public void Control_Characters_Are_Stripped_From_Free_Text()
    {
        ExtractedValueParser.SanitizeText("Ahmed\u0000\u0007 Ali", 128).ShouldBe("Ahmed Ali");
        ExtractedValueParser.SanitizeText("   ", 128).ShouldBeNull();
    }
}
