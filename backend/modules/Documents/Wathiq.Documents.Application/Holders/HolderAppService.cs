using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Wathiq.Documents.Documents;
using Wathiq.Documents.Permissions;

namespace Wathiq.Documents.Holders;

[Authorize(DocumentsPermissions.Holders.Default)]
public class HolderAppService : DocumentsAppServiceBase, IHolderAppService
{
    private readonly IRepository<Holder, Guid> _holders;
    private readonly IRepository<Document, Guid> _documents;
    private readonly HolderManager _holderManager;

    public HolderAppService(
        IRepository<Holder, Guid> holders,
        IRepository<Document, Guid> documents,
        HolderManager holderManager)
    {
        _holders = holders;
        _documents = documents;
        _holderManager = holderManager;
    }

    public async Task<ListResultDto<HolderDto>> GetListAsync()
    {
        // "Created on first use": listing is the first thing every client does, so the self
        // holder materialises here instead of at registration (Documents can't hook Identity).
        await _holderManager.EnsureSelfHolderAsync(
            CurrentUser.GetId(),
            CurrentUser.Name ?? CurrentUser.UserName ?? "Me");

        var holders = await _holders.GetListAsync(h => h.UserId == CurrentUser.GetId());

        var items = holders
            .OrderByDescending(h => h.IsSelf).ThenBy(h => h.FullName)
            .Select(ToDto)
            .ToList();

        return new ListResultDto<HolderDto>(items);
    }

    [Authorize(DocumentsPermissions.Holders.Create)]
    public async Task<HolderDto> CreateAsync(CreateHolderDto input)
    {
        if (input.Relation == HolderRelation.Self)
        {
            throw new BusinessException(DocumentsErrorCodes.SelfHolderIsAutomatic);
        }

        var holder = new Holder(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            input.FullName,
            input.Relation,
            input.BirthDate);

        await _holders.InsertAsync(holder, autoSave: true);
        return ToDto(holder);
    }

    [Authorize(DocumentsPermissions.Holders.Update)]
    public async Task<HolderDto> UpdateAsync(Guid id, UpdateHolderDto input)
    {
        var holder = await GetOwnedAsync(id);

        holder.SetFullName(input.FullName).SetBirthDate(input.BirthDate);

        await _holders.UpdateAsync(holder, autoSave: true);
        return ToDto(holder);
    }

    [Authorize(DocumentsPermissions.Holders.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var holder = await GetOwnedAsync(id);

        if (holder.IsSelf)
        {
            throw new BusinessException(DocumentsErrorCodes.CannotDeleteSelfHolder);
        }

        // Guard the Restrict FK with a friendly error instead of letting SQL throw at commit.
        if (await _documents.AnyAsync(d => d.HolderId == id))
        {
            throw new BusinessException(DocumentsErrorCodes.HolderHasDocuments);
        }

        await _holders.DeleteAsync(holder, autoSave: true);
    }

    /// <summary>Not-found (404), not forbidden (403): a 403 would confirm the id exists for someone else.</summary>
    private async Task<Holder> GetOwnedAsync(Guid id)
    {
        var holder = await _holders.GetAsync(id);
        if (holder.UserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Holder), id);
        }

        return holder;
    }

    private static HolderDto ToDto(Holder h) => new()
    {
        Id = h.Id,
        FullName = h.FullName,
        Relation = h.Relation,
        BirthDate = h.BirthDate,
        IsSelf = h.IsSelf
    };
}
