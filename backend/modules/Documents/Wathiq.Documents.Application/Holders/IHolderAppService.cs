using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Wathiq.Documents.Holders;

public interface IHolderAppService : IApplicationService
{
    Task<ListResultDto<HolderDto>> GetListAsync();
    Task<HolderDto> CreateAsync(CreateHolderDto input);
    Task<HolderDto> UpdateAsync(Guid id, UpdateHolderDto input);
    Task DeleteAsync(Guid id);
}
