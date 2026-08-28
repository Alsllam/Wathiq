using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Ai.Usage;
using Xunit;

namespace Wathiq.Ai;

/* THE 3.4 smoke: one real round-trip through the keyed client resolved from DI - so a pass
 * proves routing, the OllamaSharp adapter, the cap/ledger decorator and the model itself, in
 * one go. Gated by [OllamaFact]; concrete class in EFCore.Tests. */
public abstract class AiRoundTripSmokeTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    [OllamaFact]
    public async Task Extraction_Client_Round_Trips_Against_Live_Ollama()
    {
        var userId = Guid.NewGuid();
        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        // Keyed resolution: exactly what 3.6's extractor will do.
        var client = ServiceProvider.GetRequiredKeyedService<IChatClient>(AiConsts.ExtractionClientKey);

        ChatResponse response;
        using (accessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
                   [new Claim(AbpClaimTypes.UserId, userId.ToString())], "test"))))
        {
            response = await client.GetResponseAsync(
                [new ChatMessage(ChatRole.User, "Reply with exactly the word: ready")]);
        }

        response.Text.ShouldNotBeNullOrWhiteSpace();

        // The decorator was in the path: the live call is on the books (FR-AI-004).
        var usages = GetRequiredService<IRepository<AiUsage, Guid>>();
        (await usages.GetListAsync(u => u.UserId == userId)).Count.ShouldBe(1);
    }
}
