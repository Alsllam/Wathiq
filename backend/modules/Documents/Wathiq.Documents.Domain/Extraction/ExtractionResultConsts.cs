namespace Wathiq.Documents.Extraction;

public static class ExtractionResultConsts
{
    public const int MaxProviderLength = 32;
    public const int MaxModelLength = 64;
    // database.md planned 16, but the real id "extract-document@v1" is 19 chars - doc corrected this step.
    public const int MaxPromptVersionLength = 32;
}
