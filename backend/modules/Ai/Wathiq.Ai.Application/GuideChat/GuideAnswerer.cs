using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Wathiq.Shared.GuideChat;

namespace Wathiq.Ai.GuideChat;

/// <summary>
/// The Ai module's implementation of the Guides→Ai seam. The KEYED guides client means the cap
/// and ledger (purpose GuideChat, FR-AI-004) are in every call's path - and unlike extraction,
/// this client's provider is config-swappable (guide content is public, C1 allows cloud here).
/// </summary>
public class GuideAnswerer : IGuideAnswerer, ITransientDependency
{
    public const string PromptVersion = "guides-chat@v1";

    private static readonly string PromptText = LoadPrompt();

    private readonly IChatClient _chatClient;
    private readonly AiOptions _options;

    public GuideAnswerer(
        [FromKeyedServices(AiConsts.GuidesClientKey)] IChatClient chatClient,
        AiOptions options)
    {
        _chatClient = chatClient;
        _options = options;
    }

    public async Task<GuideModelAnswer> AnswerAsync(
        string question, IReadOnlyList<GuideExcerpt> excerpts, CancellationToken cancellationToken = default)
    {
        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json,
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                [AiConsts.PromptVersionOptionKey] = PromptVersion
            }
        };

        // Excerpts and question in ONE user message: the model sees labels, never chunk ids.
        var user = new StringBuilder();
        foreach (var excerpt in excerpts)
        {
            user.AppendLine($"[{excerpt.Label}]");
            user.AppendLine(excerpt.Text);
            user.AppendLine();
        }
        user.AppendLine($"Question: {question}");

        var stopwatch = Stopwatch.StartNew();
        var response = await _chatClient.GetResponseAsync(
        [
            new ChatMessage(ChatRole.System, PromptText),
            new ChatMessage(ChatRole.User, user.ToString())
        ], chatOptions, cancellationToken);
        stopwatch.Stop();

        var (answer, citations) = GuideAnswerParser.Parse(response.Text);

        return new GuideModelAnswer
        {
            AnswerText = answer,
            CitedLabels = citations,
            Provider = _options.Guides.Provider,
            Model = _options.Guides.Model,
            PromptVersion = PromptVersion,
            DurationMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue)
        };
    }

    private static string LoadPrompt()
    {
        var assembly = typeof(GuideAnswerer).Assembly;
        const string name = "Wathiq.Ai.Prompts.guides-chat.v1.txt";   // pinned in the csproj (the 3.6 lesson)
        using var stream = assembly.GetManifestResourceStream(name)
                           ?? throw new InvalidOperationException($"Embedded prompt '{name}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
