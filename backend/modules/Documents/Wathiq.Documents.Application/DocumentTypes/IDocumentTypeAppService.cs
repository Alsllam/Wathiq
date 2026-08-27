using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wathiq.Documents.DocumentTypes;

// IApplicationService is what later lets Wathiq.HttpApi.Client generate typed proxies from this
// interface; the auto API controller itself is generated from the implementing class.
public interface IDocumentTypeAppService : IApplicationService
{
    Task<ListResultDto<DocumentTypeDto>> GetListAsync();
}
