using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Ai.Extraction;
using Wathiq.Shared.Extraction;
using Xunit;
using Xunit.Abstractions;

namespace Wathiq.Ai.Evals;

/* The eval harness (3.8). The set and the scorer are always-on tests; the live scoring run is
 * the prompt's regression suite and needs a model, so it rides the 3.4 gate (dev box,
 * WATHIQ_OLLAMA_SMOKE=1). Concrete class in EFCore.Tests. */
public abstract class ExtractionEvalTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ITestOutputHelper _output;

    protected ExtractionEvalTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void The_Set_Is_Well_Formed_And_Bilingual()
    {
        var cases = EvalSet.Load();

        cases.Count.ShouldBe(10);
        cases.Select(c => c.Id).Distinct().Count().ShouldBe(10);
        cases.Count(c => c.Lang == "ar").ShouldBe(5);
        cases.Count(c => c.Lang == "en").ShouldBe(5);
        cases.ShouldAllBe(c => !string.IsNullOrWhiteSpace(c.OcrText));

        // Every non-null expected value must itself survive the validators - a ground truth the
        // pipeline is FORBIDDEN from emitting would make its case unwinnable by construction.
        foreach (var c in cases.Where(c => c.Expected.Number != null))
        {
            ExtractedValueParser.TryParseDocumentNumber(c.Expected.Number).ShouldBe(c.Expected.Number, c.Id);
        }
    }

    [Fact]
    public void The_Scorer_Counts_Nulls_As_Fields_To_Get_Right()
    {
        var @case = EvalSet.Load().Single(c => c.Id == "en-not-a-document");

        // Hallucinating a number where the truth is "nothing" scores 2/3, not 3/3.
        var hallucinated = new DocumentDataProposal { Number = "OFFER-800" };
        EvalScorer.Score(@case, hallucinated).ShouldBe((2, 3));
        EvalScorer.Score(@case, new DocumentDataProposal()).ShouldBe((3, 3));
    }

    [OllamaFact]
    public async Task Extraction_Eval_Scores_The_Live_Model()
    {
        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        // The REAL registered extractor over the REAL keyed client - the exact production path.
        var extractor = new DocumentDataExtractor(
            ServiceProvider.GetRequiredKeyedService<IChatClient>(AiConsts.ExtractionClientKey),
            GetRequiredService<AiOptions>());

        using (accessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
                   [new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString())], "test"))))
        {
            int matched = 0, total = 0;
            foreach (var @case in EvalSet.Load())
            {
                var proposal = await extractor.ExtractAsync(@case.OcrText);
                var (m, t) = EvalScorer.Score(@case, proposal);
                matched += m;
                total += t;
                _output.WriteLine($"{@case.Id,-28} {m}/{t}  number={proposal.Number ?? "-"} " +
                                  $"issue={proposal.IssueDate?.ToString("yyyy-MM-dd") ?? "-"} " +
                                  $"expiry={proposal.ExpiryDate?.ToString("yyyy-MM-dd") ?? "-"}");
            }

            var accuracy = (double)matched / total;
            _output.WriteLine($"FIELD ACCURACY {matched}/{total} = {accuracy:P1} (prompt {DocumentDataExtractor.PromptVersion})");

            // v1 records a baseline; the floor only rejects a catastrophically broken pipeline.
            // Tighten toward the recorded baseline when comparing prompt versions (ai-safety.md §6).
            accuracy.ShouldBeGreaterThan(0.5, "the live model + parsers lost more than half the fields");
        }
    }
}
