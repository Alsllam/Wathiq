using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Wathiq.Documents.Documents;

public interface IDocumentAppService : IApplicationService
{
    Task<DocumentDto> GetAsync(Guid id);
    Task<PagedResultDto<DocumentDto>> GetListAsync(GetDocumentListInput input);
    Task<DocumentDto> CreateAsync(CreateDocumentDto input);
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto input);
    Task<DocumentDto> RenewAsync(Guid id, RenewDocumentDto input);
    Task DeleteAsync(Guid id);

    // IRemoteStreamContent: ABP's streaming file abstraction - multipart form on the way in,
    // a file response on the way out; no byte[] round trip through JSON.
    Task<AttachmentDto> UploadAttachmentAsync(Guid id, IRemoteStreamContent file);
    Task<IRemoteStreamContent> GetAttachmentContentAsync(Guid id, Guid attachmentId);
    Task DeleteAttachmentAsync(Guid id, Guid attachmentId);
}
