using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Documents;
using Wathiq.Guides.Data;
using Wathiq.Guides.Embedding;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Guides;

/* FR-GDE-003 with a scripted model: publish enqueues (post-commit), the job chunks + embeds +
 * stores, and a re-run rebuilds instead of duplicating. Concrete class in EFCore.Tests. */
public abstract class GuideEmbedPipelineTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IGuideAdminAppService _admin;
    private readonly IRepository<Guide, Guid> _guides;
    private readonly IRepository<GuideVersion, Guid> _versions;
    private readonly IRepository<GuideChunk, Guid> _chunks;
    private readonly RecordingBackgroundJobManager _queue;
    private readonly GuideEmbedJob _job;

    protected GuideEmbedPipelineTests()
    {
        _admin = GetRequiredService<IGuideAdminAppService>();
        _guides = GetRequiredService<IRepository<Guide, Guid>>();
        _versions = GetRequiredService<IRepository<GuideVersion, Guid>>();
        _chunks = GetRequiredService<IRepository<GuideChunk, Guid>>();
        _queue = GetRequiredService<RecordingBackgroundJobManager>();
        _job = GetRequiredService<GuideEmbedJob>();
        _queue.Enqueued.Clear();   // the startup seed publishes twice - scope to THIS test's work
    }

    [Fact]
    public async Task Publish_Enqueues_The_Embed_Job_For_That_Version()
    {
        var draft = await CreateDraftAsync();

        await _admin.PublishAsync(draft.Id);

        _queue.Enqueued.OfType<GuideEmbedArgs>()
            .ShouldContain(a => a.GuideVersionId == draft.Id);
    }

    [Fact]
    public async Task Job_Stores_Chunks_With_Vectors_And_Model_And_Rebuilds_Idempotently()
    {
        var draft = await CreateDraftAsync();
        await _admin.PublishAsync(draft.Id);

        await _job.ExecuteAsync(new GuideEmbedArgs { GuideVersionId = draft.Id });

        var stored = await _chunks.GetListAsync(c => c.GuideVersionId == draft.Id);
        stored.Count.ShouldBe(3);                             // facts + body + steps for this content
        stored.Select(c => c.ChunkNo).OrderBy(n => n).ShouldBe([1, 2, 3]);
        stored.ShouldAllBe(c => c.EmbeddingModel == "fake-embed");

        // Stored bytes are exactly the converter's encoding of the fake's deterministic vector.
        var steps = stored.Single(c => c.Text.StartsWith("1. "));
        steps.Embedding.ShouldBe(EmbeddingConverter.ToBytes(FakeEmbeddingGenerator.VectorFor(steps.Text)));

        // Re-run (a Hangfire retry after a crash): rebuild, not duplicate.
        await _job.ExecuteAsync(new GuideEmbedArgs { GuideVersionId = draft.Id });
        (await _chunks.GetListAsync(c => c.GuideVersionId == draft.Id)).Count.ShouldBe(3);
    }

    [Fact]
    public async Task Job_Skips_A_Draft_Version_Without_Writing()
    {
        var draft = await CreateDraftAsync();   // never published

        await _job.ExecuteAsync(new GuideEmbedArgs { GuideVersionId = draft.Id });

        (await _chunks.GetListAsync(c => c.GuideVersionId == draft.Id)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Rebuild_Enqueues_For_Published_And_Refuses_Drafts()
    {
        var draft = await CreateDraftAsync();

        // The manual lever exists for versions published before the pipeline (the seed) and
        // model swaps - but a draft must be refused, not silently queued.
        (await Should.ThrowAsync<Volo.Abp.BusinessException>(() => _admin.RebuildEmbeddingsAsync(draft.Id)))
            .Code.ShouldBe(GuidesErrorCodes.VersionNotPublished);

        await _admin.PublishAsync(draft.Id);
        _queue.Enqueued.Clear();

        await _admin.RebuildEmbeddingsAsync(draft.Id);

        _queue.Enqueued.OfType<GuideEmbedArgs>().ShouldContain(a => a.GuideVersionId == draft.Id);
    }

    private async Task<GuideVersionDto> CreateDraftAsync()
    {
        var guide = await _admin.CreateAsync(new CreateGuideDto
        {
            Slug = $"embed-{Guid.NewGuid():N}"[..24],
            TitleAr = "دليل التضمين",
            TitleEn = "Embedding guide"
        });

        return await _admin.CreateVersionAsync(new CreateGuideVersionDto
        {
            GuideId = guide.Id,
            Language = "ar",
            BodyMarkdown = "## الرسوم\nالرسوم ثلاثمئة ريال تُسدَّد عبر قنوات البنوك.",
            LastVerifiedAt = new DateOnly(2026, 9, 1),
            Fees = "300 ريال",
            Steps = ["سدّد الرسوم", "قدّم الطلب"]
        });
    }
}
