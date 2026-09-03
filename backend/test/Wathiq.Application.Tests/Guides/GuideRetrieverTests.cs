using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;
using Wathiq.Guides.Embedding;
using Wathiq.Guides.Guides;
using Wathiq.Guides.Retrieval;
using Xunit;

namespace Wathiq.Guides;

/* Retrieval over real SQL-stored vectors with the deterministic fake generator: the question
 * "is my exact chunk text" must score ~1.0 and rank first; floors, topK, the served-corpus
 * scope and the model filter are all observable behavior here. Concrete class in EFCore.Tests. */
public abstract class GuideRetrieverTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IGuideAdminAppService _admin;
    private readonly IGuideRetriever _retriever;
    private readonly IRepository<GuideChunk, Guid> _chunks;
    private readonly GuideEmbedJob _job;
    private readonly GuideChunkCache _cache;

    protected GuideRetrieverTests()
    {
        _admin = GetRequiredService<IGuideAdminAppService>();
        _retriever = GetRequiredService<IGuideRetriever>();
        _chunks = GetRequiredService<IRepository<GuideChunk, Guid>>();
        _job = GetRequiredService<GuideEmbedJob>();
        _cache = GetRequiredService<GuideChunkCache>();
        _cache.Invalidate();   // other test classes in the collection may have warmed a stale corpus
    }

    [Fact]
    public async Task Exact_Text_Ranks_First_With_A_Near_Perfect_Score()
    {
        var stepsChunkText = await PublishAndEmbedAsync();

        var matches = await _retriever.RetrieveAsync(stepsChunkText);

        matches.ShouldNotBeEmpty();
        matches[0].Text.ShouldBe(stepsChunkText);          // same text → same fake vector → cosine 1
        matches[0].Score.ShouldBeGreaterThan(0.999);
        matches.Select(m => m.Score).ShouldBeInOrder(SortDirection.Descending);
        matches[0].GuideVersionId.ShouldNotBe(Guid.Empty); // the citation anchor rides every match
    }

    [Fact]
    public async Task Floor_And_TopK_Bound_The_Result()
    {
        var text = await PublishAndEmbedAsync();

        (await _retriever.RetrieveAsync(text, similarityFloor: 1.01)).ShouldBeEmpty();   // nothing clears an impossible floor
        (await _retriever.RetrieveAsync(text, topK: 1)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Chunks_From_Another_Embedding_Model_Are_Invisible()
    {
        var text = await PublishAndEmbedAsync();
        var alien = (await _chunks.GetListAsync()).First(c => c.Text == text);

        // Same text, same vector, different declared model: a different space entirely.
        await _chunks.InsertAsync(new GuideChunk(
            GetRequiredService<IGuidGenerator>().Create(), alien.GuideVersionId, 99,
            alien.Text, alien.Embedding, "other-model", alien.TokenCount), autoSave: true);
        _cache.Invalidate();

        var matches = await _retriever.RetrieveAsync(text);

        matches.ShouldAllBe(m => m.ChunkNo != 99);   // never compared, despite a would-be perfect score
    }

    [Fact]
    public async Task A_Superseded_Version_Drops_Out_Of_The_Corpus()
    {
        var guideId = Guid.Empty;
        var v1Steps = await PublishAndEmbedAsync(g => guideId = g);

        // Re-author: v2 published and embedded → v1's chunks stay in SQL but leave the corpus.
        var v2 = await _admin.CreateVersionAsync(new CreateGuideVersionDto
        {
            GuideId = guideId,
            Language = "ar",
            BodyMarkdown = "## المحدث\nمحتوى جديد كليًا.",
            LastVerifiedAt = new DateOnly(2026, 9, 3),
            Steps = ["الخطوة الجديدة"]
        });
        await _admin.PublishAsync(v2.Id);
        await _job.ExecuteAsync(new GuideEmbedArgs { GuideVersionId = v2.Id });

        var matches = await _retriever.RetrieveAsync(v1Steps);

        matches.ShouldAllBe(m => m.Text != v1Steps);          // the old steps chunk is not served
        (await _chunks.GetListAsync()).ShouldContain(c => c.Text == v1Steps);   // ...but still exists for citations
    }

    /// <summary>Creates+publishes a fresh guide, runs the embed job, returns the steps chunk's exact text.</summary>
    private async Task<string> PublishAndEmbedAsync(Action<Guid>? captureGuideId = null)
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var guide = await _admin.CreateAsync(new CreateGuideDto
        {
            Slug = $"retrieve-{marker}",
            TitleAr = "دليل الاسترجاع",
            TitleEn = "Retrieval guide"
        });
        captureGuideId?.Invoke(guide.Id);

        var draft = await _admin.CreateVersionAsync(new CreateGuideVersionDto
        {
            GuideId = guide.Id,
            Language = "ar",
            BodyMarkdown = $"## الرسوم {marker}\nثلاثمئة ريال.",
            LastVerifiedAt = new DateOnly(2026, 9, 1),
            Steps = [$"سدّد الرسوم {marker}", "قدّم الطلب"]
        });
        await _admin.PublishAsync(draft.Id);
        await _job.ExecuteAsync(new GuideEmbedArgs { GuideVersionId = draft.Id });

        return $"1. سدّد الرسوم {marker}\n2. قدّم الطلب";
    }
}
