namespace Wathiq.Guides.Guides;

public static class GuideConsts
{
    public const int MaxSlugLength = 64;
    public const int MaxTitleLength = 256;

    public const int LanguageLength = 2;
    public const int MaxRequiredDocumentsLength = 1024;
    public const int MaxFeesLength = 256;
    public const int MaxLocationLength = 512;

    public const int MaxStepTextLength = 2048;

    /// <summary>bge-m3 = 1024 float32s = 4096 bytes; the column and the smoke test both pin it.</summary>
    public const int EmbeddingByteLength = 4096;
    public const int MaxEmbeddingModelLength = 64;

    public static readonly string[] SupportedLanguages = ["ar", "en"];
}
