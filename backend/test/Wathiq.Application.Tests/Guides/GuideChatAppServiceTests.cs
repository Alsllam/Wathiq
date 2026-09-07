using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Wathiq.Guides.Chat;
using Wathiq.Guides.Embedding;
using Wathiq.Guides.Guides;
using Wathiq.Guides.Retrieval;
using Xunit;

namespace Wathiq.Guides;

/* FR-GDE-004 + FR-AI-003 through the real stack with a scripted model: the answer path carries
 * validated citations and freshness; every failure mode (no retrieval, model refusal,
 * hallucinated citations) lands on the honest refusal. Concrete class in EFCore.Tests. */
public abstract class GuideChatAppServiceTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IChatAppService _chat;
    private readonly IGuideAdminAppService _admin;
    private readonly GuideEmbedJob _job;
    private readonly GuideChunkCache _cache;
    private readonly FakeGuideAnswerer _answerer;

    protected GuideChatAppServiceTests()
    {
        _chat = GetRequiredService<IChatAppService>();
        _admin = GetRequiredService<IGuideAdminAppService>();
        _job = GetRequiredService<GuideEmbedJob>();
        _cache = GetRequiredService<GuideChunkCache>();
        _answerer = GetRequiredService<FakeGuideAnswerer>();
        _cache.Invalidate();
        _answerer.Calls.Clear();
    }

    [Fact]
    public async Task Grounded_Answer_Carries_Validated_Citations_And_Freshness()
    {
        var (slug, stepsText) = await PublishAndEmbedAsync();
        _answerer.NextAnswer = "سدّد الرسوم ثم قدّم الطلب عبر أبشر.";
        _answerer.NextCitations = ["C1"];

        var response = await _chat.AskAsync(new GuideChatRequestDto { Question = stepsText });

        response.Answered.ShouldBeTrue();
        response.Answer.ShouldBe("سدّد الرسوم ثم قدّم الطلب عبر أبشر.");
        response.HallucinatedCitationsDropped.ShouldBeFalse();
        var citation = response.Citations.ShouldHaveSingleItem();
        citation.GuideSlug.ShouldBe(slug);                         // resolvable back to a real guide
        citation.ChunkId.ShouldNotBe(Guid.Empty);
        citation.LastVerifiedAt.ShouldBe(new DateOnly(2026, 9, 1));
        response.LastVerifiedAt.ShouldBe(new DateOnly(2026, 9, 1));   // Vision R2 on the answer itself

        // The model saw labels and text - never a chunk id.
        _answerer.Calls.ShouldHaveSingleItem().Excerpts[0].Label.ShouldBe("C1");
    }

    [Fact]
    public async Task Hallucinated_Citations_Are_Dropped_With_A_Warning()
    {
        var (_, stepsText) = await PublishAndEmbedAsync();
        _answerer.NextAnswer = "إجابة";
        _answerer.NextCitations = ["C1", "C9"];   // C9 was never retrieved

        var response = await _chat.AskAsync(new GuideChatRequestDto { Question = stepsText });

        response.Answered.ShouldBeTrue();
        response.Citations.Count.ShouldBe(1);                      // only C1 survived
        response.HallucinatedCitationsDropped.ShouldBeTrue();      // and the drop is not silent
    }

    [Fact]
    public async Task An_Answer_With_Only_Invented_Citations_Is_Refused_Whole()
    {
        var (_, stepsText) = await PublishAndEmbedAsync();
        _answerer.NextAnswer = "إجابة غير مدعومة";
        _answerer.NextCitations = ["C9"];   // nothing real survives → ungrounded by definition

        var response = await _chat.AskAsync(new GuideChatRequestDto { Question = stepsText });

        response.Answered.ShouldBeFalse();
        response.Answer.ShouldBeNull();
        response.Message.ShouldNotBeNullOrWhiteSpace();
        response.HallucinatedCitationsDropped.ShouldBeTrue();
    }

    [Fact]
    public async Task Model_Refusal_Passes_Through_As_An_Honest_No_Answer()
    {
        var (_, stepsText) = await PublishAndEmbedAsync();
        _answerer.NextAnswer = null;

        var response = await _chat.AskAsync(new GuideChatRequestDto { Question = stepsText });

        response.Answered.ShouldBeFalse();
        response.Citations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Nothing_Above_The_Floor_Refuses_Without_Calling_The_Model()
    {
        await PublishAndEmbedAsync();

        // The sentinel embeds to a zero vector: cosine 0 with everything, below any floor.
        var response = await _chat.AskAsync(new GuideChatRequestDto { Question = FakeEmbeddingGenerator.NoMatchSentinel });

        response.Answered.ShouldBeFalse();
        _answerer.Calls.ShouldBeEmpty();   // no retrieval hit → the model is never even asked
    }

    private async Task<(string Slug, string StepsText)> PublishAndEmbedAsync()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var guide = await _admin.CreateAsync(new CreateGuideDto
        {
            Slug = $"chat-{marker}",
            TitleAr = "دليل المحادثة",
            TitleEn = "Chat guide"
        });
        var draft = await _admin.CreateVersionAsync(new CreateGuideVersionDto
        {
            GuideId = guide.Id,
            Language = "ar",
            BodyMarkdown = $"## نص {marker}",
            LastVerifiedAt = new DateOnly(2026, 9, 1),
            Steps = [$"سدّد {marker}", "قدّم الطلب"]
        });
        await _admin.PublishAsync(draft.Id);
        await _job.ExecuteAsync(new GuideEmbedArgs { GuideVersionId = draft.Id });

        return (guide.Slug, $"1. سدّد {marker}\n2. قدّم الطلب");
    }
}
