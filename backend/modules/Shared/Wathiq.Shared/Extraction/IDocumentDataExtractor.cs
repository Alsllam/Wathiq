using System.Threading;
using System.Threading.Tasks;

namespace Wathiq.Shared.Extraction;

/// <summary>
/// The Documents→Ai seam (same pattern as IOcrService, opposite direction of flow): Documents
/// hands over OCR text, gets a validated proposal back, and never learns which model or prompt
/// produced it. The implementation lives in the Ai module behind the keyed extraction client -
/// so the FR-AI-002 privacy wall and the FR-AI-004 cap/ledger decorator are always in its path.
/// </summary>
public interface IDocumentDataExtractor
{
    Task<DocumentDataProposal> ExtractAsync(string ocrText, CancellationToken cancellationToken = default);
}
