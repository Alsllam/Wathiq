using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Ai;
using Wathiq.Ai.GuideChat;
using Wathiq.Guides.Data;
using Wathiq.Guides.Guides;
using Xunit;
using Xunit.Abstractions;

namespace Wathiq.Guides.Evals;

/* The RAG eval harness. Set shape and scoring intent are always-on; the live run - real bge-m3
 * retrieval over the REAL seeded guide + real qwen through the REAL GuideAnswerer, replicating
 * the service's citation policy - rides the 3.4 gate (dev box, WATHIQ_OLLAMA_SMOKE=1).
 * Concrete class in EFCore.Tests. */
public abstract class RagEvalTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ITestOutputHelper _output;

    protected RagEvalTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void The_Set_Is_Well_Formed_Bilingual_And_Contains_Must_Refuse_Cases()
    {
        var cases = RagEvalSet.Load();

        cases.Count.ShouldBe(10);
        cases.Select(c => c.Id).Distinct().Count().ShouldBe(10);
        cases.Count(c => c.Lang == "ar").ShouldBe(5);
        cases.Count(c => c.Lang == "en").ShouldBe(5);
        cases.ShouldAllBe(c => c.Expect == "answer" || c.Expect == "refuse");

        // Refusal is a scored behavior, not an error state (3.8's null-is-a-label, conversation level):
        // a set without must-refuse cases could not distinguish a grounded assistant from a confident liar.
        cases.Count(c => c.Expect == "refuse").ShouldBe(4);
        cases.Where(c => c.Expect == "answer").ShouldAllBe(c => c.MustCiteSlug == GuidesDataSeedContributor.PassportRenewalSlug);
    }

    [OllamaFact]
    public async Task Rag_Eval_Scores_The_Live_Pipeline()
    {
        // REAL corpus: the seeded guide as readers get it, chunked by the production chunker,
        // embedded by the production model - not fixtures.
        var versions = await GetRequiredService<IRepository<GuideVersion, Guid>>()
            .GetListAsync(v => v.PublishedAt != null);
        var guides = await GetRequiredService<IRepository<Guide, Guid>>()
            .GetListAsync(g => g.Slug == GuidesDataSeedContributor.PassportRenewalSlug);
        var seeded = versions.Where(v => v.GuideId == guides.Single().Id).ToList();
        seeded.Count.ShouldBe(2);

        IEmbeddingGenerator<string, Embedding<float>> embedder =
            new OllamaApiClient(new Uri("http://localhost:11434"), "bge-m3");

        var corpus = new List<(string Text, float[] Vector)>();
        foreach (var version in seeded)
        {
            var drafts = GuideChunker.Chunk(
                version.BodyMarkdown, version.Steps.OrderBy(s => s.StepNo).Select(s => s.Text),
                version.RequiredDocuments, version.Fees, version.Location);
            var embedded = await embedder.GenerateAsync(drafts.Select(d => d.Text).ToList());
            corpus.AddRange(drafts.Zip(embedded, (d, e) => (d.Text, e.Vector.ToArray())));
        }
        _output.WriteLine($"corpus: {corpus.Count} chunks from {seeded.Count} seeded versions");

        // REAL answerer over the REAL keyed guides client (the exact production path).
        var answerer = new GuideAnswerer(
            ServiceProvider.GetRequiredKeyedService<IChatClient>(AiConsts.GuidesClientKey),
            GetRequiredService<AiOptions>());

        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        using var _ = accessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString())], "test")));

        int answerOk = 0, answerTotal = 0, refuseOk = 0, refuseTotal = 0, cleanCitations = 0;
        foreach (var @case in RagEvalSet.Load())
        {
            // Retrieval, replicating GuideRetriever's math and defaults (floor 0.5, top 4).
            var questionVector = (await embedder.GenerateAsync([@case.Question]))[0].Vector.ToArray();
            var top = corpus
                .Select(c => (c.Text, Score: VectorMath.CosineSimilarity(questionVector, c.Vector)))
                .Where(m => m.Score >= 0.5)
                .OrderByDescending(m => m.Score)
                .Take(4)
                .ToList();

            bool answered;
            var dropped = false;
            if (top.Count == 0)
            {
                answered = false;
            }
            else
            {
                var labels = top.Select((_, i) => $"C{i + 1}").ToList();
                var model = await answerer.AnswerAsync(
                    @case.Question, labels.Zip(top, (l, t) => new Wathiq.Shared.GuideChat.GuideExcerpt(l, t.Text)).ToList());
                // The ChatAppService citation policy, replicated: only retrieved labels count,
                // and an answer with no surviving citation is a refusal.
                var valid = model.CitedLabels.Distinct().Where(labels.Contains).ToList();
                dropped = model.CitedLabels.Distinct().Count() != valid.Count;
                answered = model.AnswerText != null && valid.Count > 0;
            }

            var expectAnswer = @case.Expect == "answer";
            var correct = answered == expectAnswer;
            if (expectAnswer) { answerTotal++; if (correct) answerOk++; if (answered && !dropped) cleanCitations++; }
            else { refuseTotal++; if (correct) refuseOk++; }

            _output.WriteLine($"{@case.Id,-22} retrieved={top.Count} answered={answered} dropped={dropped} {(correct ? "OK" : "MISS")}");
        }

        _output.WriteLine($"ANSWER RATE {answerOk}/{answerTotal} · REFUSAL ACCURACY {refuseOk}/{refuseTotal} · " +
                          $"CLEAN CITATIONS {cleanCitations}/{answerOk} (prompt {GuideAnswerer.PromptVersion})");

        // v1 baselines (the 3.8 philosophy): floors reject a catastrophically broken pipeline;
        // tighten toward recorded numbers when comparing prompt or model versions (ai-safety.md §6).
        ((double)answerOk / answerTotal).ShouldBeGreaterThan(0.5, "the pipeline failed to answer half the answerable questions");
        ((double)refuseOk / refuseTotal).ShouldBeGreaterThan(0.5, "the pipeline answered questions the corpus cannot ground");
    }
}
