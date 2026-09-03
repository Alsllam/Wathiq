using System.Linq;
using Shouldly;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

/* The chunker is where retrieval quality is decided - and it is pure, so it gets the dense
 * test coverage the model-facing layers cannot. */
public class GuideChunkerTests
{
    [Fact]
    public void Facts_Steps_And_Body_Each_Get_Their_Own_Chunk()
    {
        var chunks = GuideChunker.Chunk(
            "## قبل أن تبدأ\nالتجديد إلكتروني بالكامل.",
            ["سدّد الرسوم", "ادخل أبشر"],
            requiredDocuments: "الهوية الوطنية", fees: "300 ريال", location: "أبشر");

        chunks.Count.ShouldBe(3);
        chunks[0].Text.ShouldContain("300 ريال");          // the facts chunk leads
        chunks[1].Text.ShouldContain("قبل أن تبدأ");
        chunks[2].Text.ShouldBe("1. سدّد الرسوم\n2. ادخل أبشر");   // steps numbered, ordered
        chunks.Select(c => c.ChunkNo).ShouldBe([1, 2, 3]);  // dense, 1-based
        chunks.ShouldAllBe(c => c.TokenCount > 0);
    }

    [Fact]
    public void Sections_Pack_Until_The_Budget_Then_Split_With_Overlap()
    {
        // Three sections, each ~200 estimated tokens: 1+2 exceed 300 so a split lands between.
        string Section(string title, string word) =>
            $"## {title}\n{string.Join(" ", Enumerable.Repeat(word, 200))}";
        var body = string.Join("\n", Section("أولاً", "كلمة"), Section("ثانياً", "لفظ"), Section("ثالثاً", "نص"));

        var chunks = GuideChunker.Chunk(body, [], null, null, null);

        chunks.Count.ShouldBeGreaterThan(1);
        // Overlap: the second chunk starts with the tail of the first (boundary facts findable from both sides).
        var tailOfFirst = chunks[0].Text.Split(' ').Last();
        chunks[1].Text.ShouldStartWith(tailOfFirst);
    }

    [Fact]
    public void Small_Guide_Stays_One_Body_Chunk()
    {
        var chunks = GuideChunker.Chunk("## الرسوم\n300 ريال.\n\n## المكان\nأبشر.", [], null, null, null);

        chunks.ShouldHaveSingleItem();   // both sections fit one budget - no pointless fragmentation
        chunks[0].Text.ShouldContain("الرسوم");
        chunks[0].Text.ShouldContain("المكان");
    }

    [Fact]
    public void Empty_Optional_Fields_Produce_No_Facts_Chunk()
    {
        var chunks = GuideChunker.Chunk("نص واحد.", [], null, null, null);

        chunks.ShouldHaveSingleItem();
        chunks[0].Text.ShouldBe("نص واحد.");
    }

    [Fact]
    public void Token_Estimate_Handles_Arabic_And_English()
    {
        // Words dominate for spaced text; chars/4 catches long unspaced strings.
        GuideChunker.EstimateTokens("تجديد جواز السفر").ShouldBe(4);   // 16 chars / 4 = 4 > 3 words
        GuideChunker.EstimateTokens(new string('x', 400)).ShouldBe(100);
    }
}
