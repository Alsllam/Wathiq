using System;

namespace Wathiq.Shared.Events;

/// <summary>
/// Cross-module contract (FR-REM-004): Documents announces "this document's expiry is now X",
/// Reminders reacts. It lives in Shared because both modules may depend on Shared and on nothing
/// else of each other (ADR-001) - the event type IS the module boundary's API.
/// Null ExpiryDate means "stop reminding" (expiry cleared, document archived or deleted).
/// </summary>
public class DocumentExpiryChangedEto
{
    public Guid OwnerUserId { get; set; }
    public Guid DocumentId { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
