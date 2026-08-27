using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace Wathiq.Documents.Holders;

/// <summary>
/// Domain service (ABP: DomainService = stateless, transient, may use repositories) owning the
/// rule "every user has exactly one self-holder". Created on first use rather than seeded:
/// Documents cannot enumerate Identity users without crossing the module boundary.
/// </summary>
public class HolderManager : DomainService
{
    private readonly IRepository<Holder, Guid> _holders;

    public HolderManager(IRepository<Holder, Guid> holders)
    {
        _holders = holders;
    }

    public async Task<Holder> EnsureSelfHolderAsync(Guid userId, string fullName)
    {
        var existing = await _holders.FindAsync(h => h.UserId == userId && h.IsSelf);
        if (existing != null)
        {
            return existing;
        }

        // GuidGenerator (from DomainService) yields sequential GUIDs - clustered-index friendly (DB3).
        var holder = new Holder(GuidGenerator.Create(), userId, fullName, HolderRelation.Self);
        return await _holders.InsertAsync(holder, autoSave: true);
    }
}
