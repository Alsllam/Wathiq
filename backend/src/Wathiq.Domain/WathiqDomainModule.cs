using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wathiq.Localization;
using Wathiq.MultiTenancy;
using Wathiq.Shared;
using System;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.SettingManagement;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.Caching;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement.OpenIddict;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Emailing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;

namespace Wathiq;

[DependsOn(
    typeof(WathiqDomainSharedModule),
    typeof(WathiqSharedModule),
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpCachingModule),
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpEmailingModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainModule),
    typeof(BlobStoringDatabaseDomainModule)
    )]
public class WathiqDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        // DB6: instants are UTC everywhere. ABP's IClock defaults to Local; setting it here (the
        // composition root's domain module) makes Clock.Now UTC for every executable that hosts us.
        Configure<Volo.Abp.Timing.AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });


        // The template's DEBUG NullEmailSender swap is gone since 2.6: dev now has a real SMTP
        // sink (smtp4dev in Docker Compose), and silently-vanishing mail hides delivery bugs.

        // OCR engine knobs (path, languages) from the "Ocr" section; defaults work where
        // `tesseract` is on PATH, so the section is optional in appsettings.
        Configure<Ocr.OcrOptions>(context.Services.GetConfiguration().GetSection("Ocr"));
    }
}
