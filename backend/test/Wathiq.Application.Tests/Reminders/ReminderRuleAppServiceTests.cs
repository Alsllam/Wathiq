using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Wathiq.Documents.DocumentTypes;
using Wathiq.Documents.Documents;
using Wathiq.Documents.Holders;
using Wathiq.Reminders.Reminders;
using Wathiq.Reminders.Rules;
using Xunit;

namespace Wathiq.Reminders;

/* The rule as a singleton resource + the update->resync contract, driven through real services
 * end to end (document created via the Documents module). Concrete in EFCore.Tests. */
public abstract class ReminderRuleAppServiceTests<TStartupModule> : WathiqApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IReminderRuleAppService _rules;
    private readonly IReminderAppService _reminders;
    private readonly IDocumentAppService _documents;
    private readonly IHolderAppService _holders;
    private readonly IDocumentTypeAppService _types;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    protected ReminderRuleAppServiceTests()
    {
        _rules = GetRequiredService<IReminderRuleAppService>();
        _reminders = GetRequiredService<IReminderAppService>();
        _documents = GetRequiredService<IDocumentAppService>();
        _holders = GetRequiredService<IHolderAppService>();
        _types = GetRequiredService<IDocumentTypeAppService>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable ActAs(Guid userId) =>
        _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "amina")
        ], "test")));

    [Fact]
    public async Task First_Get_Materialises_The_Default_Rule()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var rule = await _rules.GetAsync();

            rule.OffsetsDays.ShouldBe([90, 30, 7, 1]);
            rule.Channels.ShouldBe(ReminderChannels.Email);
            rule.TimeZoneId.ShouldBe(ReminderRuleConsts.DefaultTimeZoneId);
        }
    }

    [Fact]
    public async Task Updating_Offsets_Reshapes_Existing_Reminders()
    {
        using (ActAs(Guid.NewGuid()))
        {
            var self = (await _holders.GetListAsync()).Items.Single();
            var passport = (await _types.GetListAsync()).Items.Single(t => t.Code == "PASSPORT");
            var expiry = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1);
            await _documents.CreateAsync(new CreateDocumentDto
            {
                HolderId = self.Id, DocumentTypeId = passport.Id, ExpiryDate = expiry
            });

            (await _reminders.GetUpcomingListAsync(new PagedResultRequestDto())).TotalCount.ShouldBe(4);

            var updated = await _rules.UpdateAsync(new UpdateReminderRuleDto
            {
                OffsetsDays = [10, 10, 60],          // duplicates normalize away, order is imposed
                Channels = ReminderChannels.Email,
                TimeZoneId = "Europe/Berlin"
            });

            updated.OffsetsDays.ShouldBe([60, 10]);
            updated.TimeZoneId.ShouldBe("Europe/Berlin");

            // The resync contract: the upcoming list already matches the settings just saved.
            var upcoming = await _reminders.GetUpcomingListAsync(new PagedResultRequestDto());
            upcoming.TotalCount.ShouldBe(2);
            upcoming.Items.Select(r => r.OffsetDays).ShouldBe([60, 10]);   // soonest DueDate first
            upcoming.Items.ShouldAllBe(r => r.ExpiryDate == expiry);       // DueDate+Offset self-describes
        }
    }

    [Fact]
    public async Task Bad_Time_Zone_Is_Rejected_And_Changes_Nothing()
    {
        using (ActAs(Guid.NewGuid()))
        {
            await _rules.GetAsync();

            await Should.ThrowAsync<BusinessException>(() => _rules.UpdateAsync(new UpdateReminderRuleDto
            {
                OffsetsDays = [30],
                Channels = ReminderChannels.Email,
                TimeZoneId = "Mars/Olympus"
            }));

            (await _rules.GetAsync()).TimeZoneId.ShouldBe(ReminderRuleConsts.DefaultTimeZoneId);
        }
    }
}
