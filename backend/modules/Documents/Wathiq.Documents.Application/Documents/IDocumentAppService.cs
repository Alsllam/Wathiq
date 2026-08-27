using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wathiq.Documents.Documents;

public interface IDocumentAppService : IApplicationService
{
    Task<DocumentDto> GetAsync(Guid id);
    Task<PagedResultDto<DocumentDto>> GetListAsync(GetDocumentListInput input);
    Task<DocumentDto> CreateAsync(CreateDocumentDto input);
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto input);
    Task<DocumentDto> RenewAsync(Guid id, RenewDocumentDto input);
    Task DeleteAsync(Guid id);
}
