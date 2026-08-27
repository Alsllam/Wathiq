using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Documents.Holders;

/// <summary>The person a document belongs to: the user themself or a family member (FR-DOC-007).</summary>
public class Holder : FullAuditedAggregateRoot<Guid>
{
    // Plain Guid, no FK to AbpUsers: the Identity module owns users; we only remember the id (ADR-001, DB2).
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public HolderRelation Relation { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public bool IsSelf { get; private set; }

    private Holder()
    {
    }

    public Holder(Guid id, Guid userId, string fullName, HolderRelation relation, DateOnly? birthDate = null)
        : base(id)
    {
        UserId = userId;
        Relation = relation;
        // IsSelf is derived, not chosen: it exists as a column only for the filtered unique index.
        IsSelf = relation == HolderRelation.Self;
        SetFullName(fullName);
        SetBirthDate(birthDate);
    }

    public Holder SetFullName(string fullName)
    {
        FullName = Check.NotNullOrWhiteSpace(fullName, nameof(fullName), HolderConsts.MaxFullNameLength);
        return this;
    }

    public Holder SetBirthDate(DateOnly? birthDate)
    {
        if (birthDate.HasValue && birthDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentOutOfRangeException(nameof(birthDate), "Birth date cannot be in the future.");
        }

        BirthDate = birthDate;
        return this;
    }
}
