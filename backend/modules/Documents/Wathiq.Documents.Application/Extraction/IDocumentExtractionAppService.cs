using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Wathiq.Documents.Documents;

namespace Wathiq.Documents.Extraction;

public interface IDocumentExtractionAppService : IApplicationService
{
    /// <summary>Runs the AI over the attachment's OcrText and stores a Proposed ExtractionResult.</summary>
    Task<ExtractionProposalDto> ExtractAsync(Guid id, Guid attachmentId);

    /// <summary>Latest result for the attachment (any outcome), or null if never extracted.</summary>
    Task<ExtractionProposalDto?> GetLatestAsync(Guid id, Guid attachmentId);

    /// <summary>Applies the user's final values to the document and concludes Accepted/Edited.</summary>
    Task<DocumentDto> ConfirmAsync(Guid id, Guid extractionResultId, ConfirmExtractionDto input);

    /// <summary>Concludes Rejected; the document is untouched.</summary>
    Task RejectAsync(Guid id, Guid extractionResultId);
}
