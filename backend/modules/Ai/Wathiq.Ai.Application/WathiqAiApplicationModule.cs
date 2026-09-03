using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Wathiq.Ai.Clients;
using Wathiq.Ai.Usage;

namespace Wathiq.Ai;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(WathiqAiDomainModule)
)]
public class WathiqAiApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var aiOptions = context.Services.GetConfiguration().GetSection("Ai").Get<AiOptions>() ?? new AiOptions();
        aiOptions.Validate();   // FR-AI-002: a cloud extraction provider kills the boot, here
        context.Services.AddSingleton(aiOptions);

        // Two logical clients behind keyed DI: consumers say WHAT they need ("extraction"),
        // this module decides WHO serves it - the only place provider SDK types are allowed.
        RegisterChatClient(context, AiConsts.ExtractionClientKey, aiOptions.Extraction, AiUsagePurpose.Extraction, aiOptions);
        RegisterChatClient(context, AiConsts.GuidesClientKey, aiOptions.Guides, AiUsagePurpose.GuideChat, aiOptions);

        // The IChatClient wall's twin (5.3): consumers (the Guides embed job, 5.4's retriever)
        // inject the framework abstraction; only this line knows Ollama serves it. Validate()
        // above already refused any non-local provider for it.
        context.Services.AddTransient<IEmbeddingGenerator<string, Embedding<float>>>(
            _ => new OllamaApiClient(new Uri(aiOptions.Embeddings.Endpoint), aiOptions.Embeddings.Model));
    }

    private static void RegisterChatClient(
        ServiceConfigurationContext context, string key, AiClientOptions client, AiUsagePurpose purpose, AiOptions options)
    {
        // Transient: the decorator needs the caller's scoped ICurrentUser and repositories.
        context.Services.AddKeyedTransient<IChatClient>(key, (sp, _) =>
        {
            IChatClient inner = client.Provider.ToLowerInvariant() switch
            {
                AiConsts.OllamaProvider => new OllamaApiClient(new Uri(client.Endpoint), client.Model),
                // "groq" / "gemini" / OpenAI-compatible join here later - for the GUIDES client only.
                _ => throw new InvalidOperationException($"Unknown AI provider '{client.Provider}'.")
            };

            return new UsageTrackingChatClient(
                inner, purpose, client, options,
                sp.GetRequiredService<Volo.Abp.Users.ICurrentUser>(),
                sp.GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<AiUsage, Guid>>(),
                sp.GetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>(),
                sp.GetRequiredService<Volo.Abp.Timing.IClock>(),
                sp.GetRequiredService<Volo.Abp.Guids.IGuidGenerator>());
        });
    }
}
