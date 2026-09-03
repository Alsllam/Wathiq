using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;
using Wathiq.Guides.Events;

namespace Wathiq.Guides.Embedding;

/// <summary>
/// Fourth lap of the post-commit idiom (1.5, 3.1, 3.5): the local event fires inside the
/// publish UoW; OnCompleted defers the enqueue until the version row is actually committed,
/// so the job can never observe (or embed) a publish that rolled back.
/// </summary>
public class GuideVersionPublishedEventHandler : ILocalEventHandler<GuideVersionPublishedEto>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public GuideVersionPublishedEventHandler(
        IBackgroundJobManager backgroundJobManager,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _backgroundJobManager = backgroundJobManager;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public Task HandleEventAsync(GuideVersionPublishedEto eventData)
    {
        _unitOfWorkManager.Current!.OnCompleted(() => _backgroundJobManager.EnqueueAsync(
            new GuideEmbedArgs { GuideVersionId = eventData.GuideVersionId }));

        return Task.CompletedTask;
    }
}
