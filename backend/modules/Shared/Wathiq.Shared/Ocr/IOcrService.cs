using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wathiq.Shared.Ocr;

/// <summary>
/// Text recognition port (feeds FR-DOC-005). Same recipe as IUserContactResolver: the contract
/// lives in Shared, the Tesseract-backed implementation in the host - modules never learn which
/// OCR engine (or which binary path) is behind it. Privacy rail: implementations MUST run on
/// this server; document images never leave the machine (C1).
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extracts text from an image stream, or returns null when the content type is not one the
    /// engine can read (e.g. a PDF today) - "no text" and "can't read" both leave OcrText null.
    /// </summary>
    Task<string?> ExtractTextAsync(Stream content, string mimeType, CancellationToken cancellationToken = default);
}
