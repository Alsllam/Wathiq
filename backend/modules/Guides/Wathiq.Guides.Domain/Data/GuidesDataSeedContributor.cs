using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Wathiq.Guides.Guides;

namespace Wathiq.Guides.Data;

/// <summary>
/// Seeds ONE real guide (passport renewal, ar + en, both published) so 5.3-5.5 have genuine
/// content to chunk, embed and cite from day one - fake lorem content would make retrieval
/// quality unmeasurable. Idempotent by slug, like the DocumentTypes seed.
/// </summary>
public class GuidesDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string PassportRenewalSlug = "renew-passport";

    private readonly IRepository<Guide, Guid> _guides;
    private readonly GuideManager _guideManager;

    public GuidesDataSeedContributor(IRepository<Guide, Guid> guides, GuideManager guideManager)
    {
        _guides = guides;
        _guideManager = guideManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _guides.AnyAsync(g => g.Slug == PassportRenewalSlug))
        {
            return;
        }

        var guide = await _guideManager.CreateAsync(
            PassportRenewalSlug, "تجديد جواز السفر", "Renew a passport");

        var lastVerified = new DateOnly(2026, 9, 1);

        var ar = await _guideManager.CreateDraftAsync(
            guide.Id, "ar",
            BodyAr, lastVerified,
            requiredDocuments: "الهوية الوطنية سارية المفعول، الجواز الحالي، صورة شخصية حديثة بخلفية بيضاء",
            fees: "300 ريال (5 سنوات) أو 600 ريال (10 سنوات)",
            location: "منصة أبشر (absher.sa) — لا تتطلب زيارة مكتب الجوازات في أغلب الحالات",
            steps:
            [
                "سدّد رسوم تجديد الجواز عبر قنوات السداد البنكية قبل تقديم الطلب.",
                "سجّل الدخول إلى حسابك في منصة أبشر واختر «خدماتي» ثم «الجوازات».",
                "اختر خدمة «تجديد جواز السفر السعودي» وحدد صاحب الجواز المراد تجديده.",
                "تحقق من صحة البيانات والصورة الشخصية ثم أرسل الطلب.",
                "اختر طريقة الاستلام: توصيل عبر البريد السعودي (سبل) إلى عنوانك الوطني.",
                "تابع حالة الطلب من «طلباتي» في أبشر حتى يصلك إشعار الاستلام."
            ]);
        await _guideManager.PublishAsync(ar.Id);

        var en = await _guideManager.CreateDraftAsync(
            guide.Id, "en",
            BodyEn, lastVerified,
            requiredDocuments: "Valid national ID, the current passport, and a recent photo with a white background",
            fees: "SAR 300 (5 years) or SAR 600 (10 years)",
            location: "Absher portal (absher.sa) — no passport-office visit needed in most cases",
            steps:
            [
                "Pay the passport renewal fee through your bank's government-payments channel first.",
                "Sign in to your Absher account and open My Services, then Passports.",
                "Choose \"Renew Saudi passport\" and select the passport holder.",
                "Review your details and photo, then submit the request.",
                "Choose delivery: Saudi Post (SPL) ships the new passport to your national address.",
                "Track the request under My Requests in Absher until the pickup notification arrives."
            ]);
        // Publishing en LAST leaves it as Guide.PublishedVersionId - the public app service still
        // serves per-language (latest published in the requested language), so ar readers are unaffected.
        await _guideManager.PublishAsync(en.Id);
    }

    private const string BodyAr =
        """
        ## قبل أن تبدأ

        تجديد الجواز السعودي يتم إلكترونيًا بالكامل عبر منصة أبشر لمن أعمارهم فوق 21 عامًا،
        بشرط سداد الرسوم مسبقًا وخلو السجل من الملاحظات. الجواز الجديد يُطبع ويُرسل إلى
        عنوانك الوطني، ويبقى الجواز القديم صالحًا حتى استلام الجديد.

        ## الرسوم

        300 ريال لجواز صلاحيته 5 سنوات، و600 ريال لجواز صلاحيته 10 سنوات. السداد عبر
        قنوات البنوك (سداد) برقم الهوية الوطنية.

        ## ملاحظات

        - يشترط وجود عنوان وطني مسجّل ومحدّث لاستلام الجواز.
        - لتجديد جوازات التابعين تُستخدم الخدمة نفسها من حساب رب الأسرة.
        - إذا انتهى الجواز وأنت خارج المملكة فراجع الممثلية السعودية في بلد إقامتك.
        """;

    private const string BodyEn =
        """
        ## Before you start

        Saudi passport renewal is fully electronic through Absher for adults over 21, provided
        the fee is paid in advance and there are no holds on the record. The new passport is
        printed and shipped to your registered national address; the old one stays valid until
        you receive the new one.

        ## Fees

        SAR 300 for a 5-year passport, SAR 600 for a 10-year passport, paid through your bank's
        SADAD government payments using your national ID number.

        ## Notes

        - A registered, up-to-date national address is required for delivery.
        - Dependants' passports are renewed with the same service from the head of household's account.
        - If the passport expired while you are abroad, contact the Saudi mission in your country.
        """;
}
