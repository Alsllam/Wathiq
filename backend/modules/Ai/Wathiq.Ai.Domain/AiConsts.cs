namespace Wathiq.Ai;

public static class AiConsts
{
    public const string OllamaProvider = "ollama";

    // Keyed-service names for the two logical IChatClients.
    public const string ExtractionClientKey = "extraction";
    public const string GuidesClientKey = "guides";

    // ChatOptions.AdditionalProperties key: callers (3.6) announce the prompt version so the
    // usage ledger can record it without the tracking layer knowing about prompts.
    public const string PromptVersionOptionKey = "wathiq.promptVersion";
}
