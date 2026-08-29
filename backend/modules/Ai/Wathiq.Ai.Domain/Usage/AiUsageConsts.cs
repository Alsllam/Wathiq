namespace Wathiq.Ai.Usage;

public static class AiUsageConsts
{
    public const int MaxProviderLength = 32;
    public const int MaxModelLength = 64;
    // Was 16 - too small for real ids like "extract-document@v1" (19 chars); caught by the 3.8
    // end-to-end cap test, widened alongside ExtractionResult.PromptVersion (3.6).
    public const int MaxPromptVersionLength = 32;
}
