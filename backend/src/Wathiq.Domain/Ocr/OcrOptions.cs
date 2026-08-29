namespace Wathiq.Ocr;

/// <summary>Engine details stay with the adapter (host side) - modules only know IOcrService.</summary>
public class OcrOptions
{
    public string TesseractPath { get; set; } = "tesseract";

    /// <summary>Tesseract language stack; "ara+eng" tries both scripts on every image (D-OCR).</summary>
    public string Languages { get; set; } = "ara+eng";
}
