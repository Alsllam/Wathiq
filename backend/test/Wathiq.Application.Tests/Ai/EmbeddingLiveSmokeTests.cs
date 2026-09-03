using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Shouldly;
using Wathiq.Guides.Guides;
using Xunit;

namespace Wathiq.Ai;

/* Dev-box-only proof that the REAL generator honors the schema's contract. The DI graph swaps
 * in FakeEmbeddingGenerator, so this talks to Ollama directly - a smoke of the model+converter
 * pair, not of the wiring (GuideEmbedPipelineTests owns the wiring). */
public class EmbeddingLiveSmokeTests
{
    [OllamaFact]
    public async Task BgeM3_Embeds_Arabic_At_The_Schema_Width()
    {
        IEmbeddingGenerator<string, Embedding<float>> generator =
            new OllamaApiClient(new Uri("http://localhost:11434"), "bge-m3");

        var embeddings = await generator.GenerateAsync(["كيف أجدد جواز السفر؟"]);

        embeddings[0].Vector.Length.ShouldBe(1024);   // D2: bge-m3's dimensionality
        // The stored form fits varbinary(4096) exactly - column width proven against the live model.
        EmbeddingConverter.ToBytes(embeddings[0].Vector.Span).Length.ShouldBe(GuideConsts.EmbeddingByteLength);
    }
}
