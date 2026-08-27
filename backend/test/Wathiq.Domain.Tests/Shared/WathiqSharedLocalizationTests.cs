using Microsoft.Extensions.Localization;
using Volo.Abp.Localization;
using Shouldly;
using Volo.Abp.Modularity;
using Wathiq.Shared.Files;
using Wathiq.Shared.Localization;
using Xunit;

namespace Wathiq.Shared;

public abstract class WathiqSharedLocalizationTests<TStartupModule> : WathiqDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IStringLocalizer<WathiqSharedResource> _localizer;

    protected WathiqSharedLocalizationTests()
    {
        _localizer = GetRequiredService<IStringLocalizer<WathiqSharedResource>>();
    }

    [Theory]
    [InlineData("en", "The file exceeds the maximum allowed size.")]
    [InlineData("ar", "حجم الملف يتجاوز الحد الأقصى المسموح به.")]
    public void Should_Resolve_Error_Code_Text_In_Both_Languages(string culture, string expected)
    {
        // ABP reads CultureInfo.CurrentUICulture; a request's Accept-Language sets it in the host.
        using (CultureHelper.Use(culture))
        {
            _localizer[WathiqSharedErrorCodes.FileTooLarge].Value.ShouldBe(expected);
        }
    }
}
