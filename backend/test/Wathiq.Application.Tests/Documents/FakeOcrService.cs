using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wathiq.Shared.Ocr;

namespace Wathiq.Documents;

/// <summary>
/// Scripted engine honouring the port's contract (null for non-images); tests assert on Calls
/// the way FakeReminderChannel asserts on sends. The real Tesseract adapter lives in the host,
/// outside this test graph - which also proves the modules truly depend on the port alone.
/// </summary>
public class FakeOcrService : IOcrService
{
    public string TextToReturn { get; set; } = "WATHIQ OCR TEXT";
    public List<string> Calls { get; } = [];

    public Task<string?> ExtractTextAsync(Stream content, string mimeType, CancellationToken cancellationToken = default)
    {
        Calls.Add(mimeType);
        return Task.FromResult(mimeType.StartsWith("image/") ? TextToReturn : null);
    }
}
