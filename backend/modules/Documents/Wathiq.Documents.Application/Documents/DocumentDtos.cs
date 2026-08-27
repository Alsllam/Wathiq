using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Wathiq.Documents.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid HolderId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string? Number { get; set; }
    // The ValidityPeriod value object flattens back to two fields on the wire - clients should
    // not need to know the domain packages the pair as one type.
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DocumentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateOnly? PreviousExpiryDate { get; set; }
    /// <summary>Negative when already expired; null when no expiry is known. Server-computed so every client agrees.</summary>
    public int? DaysUntilExpiry { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateDocumentDto
{
    [Required]
    public Guid HolderId { get; set; }

    [Required]
    public Guid DocumentTypeId { get; set; }

    [StringLength(DocumentConsts.MaxNumberLength)]
    public string? Number { get; set; }

    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    [StringLength(DocumentConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}

public class UpdateDocumentDto
{
    [StringLength(DocumentConsts.MaxNumberLength)]
    public string? Number { get; set; }

    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    [StringLength(DocumentConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}

/// <summary>FR-DOC-006: the new period after renewal; the old expiry is kept by the aggregate.</summary>
public class RenewDocumentDto
{
    public DateOnly? IssueDate { get; set; }

    [Required]
    public DateOnly? ExpiryDate { get; set; }
}

public class GetDocumentListInput : PagedResultRequestDto
{
    public Guid? HolderId { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public DocumentStatus? Status { get; set; }
}
