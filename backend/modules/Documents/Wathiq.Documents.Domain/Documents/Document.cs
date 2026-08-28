using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Wathiq.Documents.Documents;

/// <summary>Aggregate root for one personal document (FR-DOC-002). Attachments live inside it.</summary>
public class Document : FullAuditedAggregateRoot<Guid>
{
    public Guid OwnerUserId { get; private set; }   // data-filter key; plain Guid, no FK to Identity (DB2)
    public Guid HolderId { get; private set; }
    public Guid DocumentTypeId { get; private set; }
    public string? Number { get; private set; }
    public ValidityPeriod Validity { get; private set; } = ValidityPeriod.None;
    public DocumentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateOnly? PreviousExpiryDate { get; private set; }

    private readonly List<Attachment> _attachments = new();
    // Read-only outside: callers cannot Add() an attachment behind the aggregate's back.
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();

    private Document()
    {
    }

    public Document(Guid id, Guid ownerUserId, Guid holderId, Guid documentTypeId, ValidityPeriod validity, string? number = null, string? notes = null)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        HolderId = holderId;
        DocumentTypeId = documentTypeId;
        Status = DocumentStatus.Active;
        SetValidity(validity);   // goes through the setter so creation announces its expiry too
        SetNumber(number);
        SetNotes(notes);
    }

    public Document SetNumber(string? number)
    {
        Number = number.IsNullOrWhiteSpace() ? null : Check.Length(number, nameof(number), DocumentConsts.MaxNumberLength);
        return this;
    }

    public Document SetNotes(string? notes)
    {
        Notes = notes.IsNullOrWhiteSpace() ? null : Check.Length(notes, nameof(notes), DocumentConsts.MaxNotesLength);
        return this;
    }

    /// <summary>Replaces the whole value object - there is no "set expiry only" because the rule needs both dates.</summary>
    public Document SetValidity(ValidityPeriod validity)
    {
        Check.NotNull(validity, nameof(validity));

        // Value-object equality gates the event: an Update that re-sends the same dates is silent.
        var expiryChanged = !validity.Equals(Validity);
        Validity = validity;

        if (expiryChanged)
        {
            PublishExpiryChanged();
        }

        return this;
    }

    /// <summary>FR-DOC-006: keep the old expiry, take the new period, back to Active.</summary>
    public Document MarkRenewed(ValidityPeriod newValidity)
    {
        PreviousExpiryDate = Validity.ExpiryDate;
        Status = DocumentStatus.Active;
        SetValidity(Check.NotNull(newValidity, nameof(newValidity)));
        return this;
    }

    public Document Archive()
    {
        Status = DocumentStatus.Archived;
        PublishRemindersStop();   // an archived document must not keep reminding (FR-REM-004)
        return this;
    }

    // AddLocalEvent queues on the aggregate; ABP publishes at SaveChanges, INSIDE the same unit
    // of work - the reminder sync and the document change commit or roll back together.
    private void PublishExpiryChanged() =>
        AddLocalEvent(new Wathiq.Shared.Events.DocumentExpiryChangedEto
        {
            OwnerUserId = OwnerUserId,
            DocumentId = Id,
            ExpiryDate = Validity.ExpiryDate
        });

    /// <summary>Deletion goes through the repository, not an aggregate method - the app service calls this first.</summary>
    public void PublishRemindersStop() =>
        AddLocalEvent(new Wathiq.Shared.Events.DocumentExpiryChangedEto
        {
            OwnerUserId = OwnerUserId,
            DocumentId = Id,
            ExpiryDate = null
        });

    public Attachment AddAttachment(Guid attachmentId, string blobKey, string mimeType, long sizeBytes, byte[] sha256)
    {
        var attachment = new Attachment(attachmentId, Id, blobKey, mimeType, sizeBytes, sha256);
        _attachments.Add(attachment);
        return attachment;
    }

    /// <summary>Returns the removed attachment's blob key so the caller can delete the file after the transaction commits.</summary>
    public string RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId)
                         ?? throw new EntityNotFoundException(typeof(Attachment), attachmentId);
        _attachments.Remove(attachment);
        return attachment.BlobKey;
    }
}
