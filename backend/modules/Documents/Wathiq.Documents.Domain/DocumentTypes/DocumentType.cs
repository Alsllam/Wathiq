using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Documents.DocumentTypes;

/// <summary>Catalogue entry: what kind of document a user can store (FR-DOC-001).</summary>
// FullAuditedAggregateRoot adds CreationTime/CreatorId, LastModification*, IsDeleted/DeletionTime
// (database.md DB4) and makes soft-delete + audit automatic through the ABP repository.
public class DocumentType : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; } = default!;
    public string NameAr { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public int? DefaultValidityMonths { get; private set; }
    public Guid? RenewalGuideId { get; private set; } // -> guides.Guide, no FK (DB2)
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core needs a parameterless constructor; keeping it private means only EF can bypass the invariants.
    private DocumentType()
    {
    }

    public DocumentType(Guid id, string code, string nameAr, string nameEn, int? defaultValidityMonths = null, int sortOrder = 0)
        : base(id)
    {
        // Check.* throws ArgumentException with the parameter name - the ABP idiom for constructor guards.
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), DocumentTypeConsts.MaxCodeLength).ToUpperInvariant();
        SetNames(nameAr, nameEn);
        SetDefaultValidity(defaultValidityMonths);
        SortOrder = sortOrder;
        IsActive = true;
    }

    public DocumentType SetNames(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), DocumentTypeConsts.MaxNameLength);
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), DocumentTypeConsts.MaxNameLength);
        return this;
    }

    public DocumentType SetDefaultValidity(int? months)
    {
        if (months is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(months), "Default validity must be a positive number of months.");
        }

        DefaultValidityMonths = months;
        return this;
    }

    public DocumentType SetRenewalGuide(Guid? guideId)
    {
        RenewalGuideId = guideId;
        return this;
    }

    public DocumentType Activate() { IsActive = true; return this; }
    public DocumentType Deactivate() { IsActive = false; return this; }
}
