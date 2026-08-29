using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;

namespace Wathiq.Documents;

/// <summary>
/// Queue stand-in: records what was enqueued instead of running anything, so a test can assert
/// "the upload scheduled OCR" without a scheduler in the graph (jobs themselves are invoked
/// directly, like ReminderDispatchJob's tests call RunAsync).
/// </summary>
public class RecordingBackgroundJobManager : IBackgroundJobManager
{
    public List<object> Enqueued { get; } = [];

    public bool IsAvailable() => true;

    public Task<string> EnqueueAsync<TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
    {
        Enqueued.Add(args!);
        return Task.FromResult(Guid.NewGuid().ToString());
    }
}
