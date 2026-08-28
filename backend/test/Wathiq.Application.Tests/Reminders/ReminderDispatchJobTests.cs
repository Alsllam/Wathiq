using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Wathiq.Reminders.Jobs;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

/* FR-REM-002 as an executable statement: the job runs TWICE in every test; the second run must
 * change nothing. Hangfire is absent on purpose - the job is a plain class. Concrete in EFCore.Tests. */
public abstract class ReminderDispatchJobTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ReminderDispatchJob _job;
    private readonly IRepository<Reminder, Guid> _reminders;
    private readonly ReminderRuleManager _ruleManager;
    private readonly FakeReminderChannel _channel;

    protected ReminderDispatchJobTests()
    {
        _job = GetRequiredService<ReminderDispatchJob>();
        _reminders = GetRequiredService<IRepository<Reminder, Guid>>();
        _ruleManager = GetRequiredService<ReminderRuleManager>();
        _channel = GetRequiredService<FakeReminderChannel>();
    }

    private async Task<Reminder> SeedReminderAsync(Guid userId, DateOnly dueDate, int offsetDays = 30)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            await _ruleManager.EnsureForUserAsync(userId);   // job requires the rule (time zone, channels)
            return await _reminders.InsertAsync(
                new Reminder(Guid.NewGuid(), userId, documentId: Guid.NewGuid(), offsetDays, dueDate));
        });
    }

    [Fact]
    public async Task Due_Reminders_Send_Once_Even_When_The_Job_Runs_Twice()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var due = await SeedReminderAsync(Guid.NewGuid(), today.AddDays(-1));
        var future = await SeedReminderAsync(Guid.NewGuid(), today.AddDays(60));

        await _job.RunAsync();
        await _job.RunAsync();   // the FR-REM-002 line: second run finds nothing Pending

        _channel.SentReminderIds.Count(id => id == due.Id).ShouldBe(1);

        var sent = await _reminders.GetAsync(due.Id);
        sent.Status.ShouldBe(ReminderStatus.Sent);
        sent.SentAt.ShouldNotBeNull();

        (await _reminders.GetAsync(future.Id)).Status.ShouldBe(ReminderStatus.Pending);

        // FR-REM-005: exactly one delivery-log row, not one per run.
        await WithUnitOfWorkAsync(async () =>
        {
            var reloaded = (await _reminders.WithDetailsAsync(r => r.DeliveryLogs))
                .Single(r => r.Id == due.Id);   // sync LINQ: keeps this project EF-free
            reloaded.DeliveryLogs.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task A_Failing_Send_Marks_Failed_And_Never_Kills_The_Run()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var poisoned = await SeedReminderAsync(Guid.NewGuid(), today);
        var healthy = await SeedReminderAsync(Guid.NewGuid(), today);

        _channel.FailWhen = r => r.Id == poisoned.Id;
        try
        {
            await _job.RunAsync();
        }
        finally
        {
            _channel.FailWhen = null;
        }

        (await _reminders.GetAsync(poisoned.Id)).Status.ShouldBe(ReminderStatus.Failed);
        (await _reminders.GetAsync(healthy.Id)).Status.ShouldBe(ReminderStatus.Sent);   // the run survived
    }
}
