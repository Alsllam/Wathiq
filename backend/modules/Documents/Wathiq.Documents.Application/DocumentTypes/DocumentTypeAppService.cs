using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Wathiq.Documents.Permissions;

namespace Wathiq.Documents.DocumentTypes;

// The [Authorize] attribute takes a *policy* name; ABP registers every defined permission as a
// policy, so this one line is the whole "gate the action" story.
[Authorize(DocumentsPermissions.DocumentTypes.Default)]
public class DocumentTypeAppService : DocumentsAppServiceBase, IDocumentTypeAppService
{
    private readonly IRepository<DocumentType, Guid> _documentTypes;

    public DocumentTypeAppService(IRepository<DocumentType, Guid> documentTypes)
    {
        _documentTypes = documentTypes;
    }

    // 4.3 product decision: the catalogue is public reference data (ar/en names, no PII) - the
    // portal shell and a future landing page read it before login. Method-level override of the
    // class-level policy; anything beyond reading the list stays permissioned.
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<ListResultDto<DocumentTypeDto>> GetListAsync()
    {
        // The catalogue is small and global - no paging (ListResultDto, not PagedResultDto).
        var types = await _documentTypes.GetListAsync(t => t.IsActive);

        var items = types
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Code)
            .Select(ToDto)
            .ToList();

        return new ListResultDto<DocumentTypeDto>(items);
    }

    // Manual mapping: the DTO shape is the contract, so writing it out keeps every exposed
    // field a deliberate decision (RenewalGuideId, audit columns stay internal).
    private static DocumentTypeDto ToDto(DocumentType t) => new()
    {
        Id = t.Id,
        Code = t.Code,
        NameAr = t.NameAr,
        NameEn = t.NameEn,
        DefaultValidityMonths = t.DefaultValidityMonths,
        SortOrder = t.SortOrder
    };
}
