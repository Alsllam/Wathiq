using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Wathiq.Reminders.Reminders;
using Wathiq.Shared.Events;

namespace Wathiq.Reminders.Events;

/// <summary>
/// The Reminders side of FR-REM-004. Local events run in the PUBLISHER's unit of work: this
/// handler's reminder rows commit atomically with the document change that caused them - and an
/// exception here rolls the document save back too, so the handler stays deliberately thin.
/// </summary>
public class DocumentExpiryChangedEventHandler : ILocalEventHandler<DocumentExpiryChangedEto>, ITransientDependency
{
    private readonly ReminderScheduler _scheduler;

    public DocumentExpiryChangedEventHandler(ReminderScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public Task HandleEventAsync(DocumentExpiryChangedEto eventData)
    {
        // Null expiry (cleared/archived/deleted) flows through as "desired schedule is empty",
        // which SyncForDocumentAsync answers by cancelling every pending reminder.
        return _scheduler.SyncForDocumentAsync(eventData.OwnerUserId, eventData.DocumentId, eventData.ExpiryDate);
    }
}
