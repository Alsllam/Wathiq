using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wathiq.Documents.Events;

namespace Wathiq.Documents.Ocr;

/// <summary>
/// Bridges the domain event to the queue. Third lap of the post-commit idiom (1.5, 3.1): local
/// event handlers run INSIDE the upload's unit of work, but Hangfire enqueues are immediate and
/// non-transactional - enqueued here directly, the job could run before the attachment row
/// commits (or run for a row that then rolls back). OnCompleted defers the enqueue to after-commit.
/// </summary>
public class AttachmentUploadedEventHandler : ILocalEventHandler<AttachmentUploadedEto>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public AttachmentUploadedEventHandler(
        IBackgroundJobManager backgroundJobManager,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _backgroundJobManager = backgroundJobManager;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public Task HandleEventAsync(AttachmentUploadedEto eventData)
    {
        _unitOfWorkManager.Current!.OnCompleted(() => _backgroundJobManager.EnqueueAsync(
            new AttachmentOcrArgs
            {
                DocumentId = eventData.DocumentId,
                AttachmentId = eventData.AttachmentId
            }));

        return Task.CompletedTask;
    }
}
