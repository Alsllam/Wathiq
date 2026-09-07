// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Arabic (`ar`).
class AppLocalizationsAr extends AppLocalizations {
  AppLocalizationsAr([String locale = 'ar']) : super(locale);

  @override
  String get appName => 'وثيق';

  @override
  String get tagline => 'مساعدك لوثائقك ومواعيدها';

  @override
  String get navGuides => 'الأدلة';

  @override
  String get navDocuments => 'الوثائق';

  @override
  String get navReminders => 'التذكيرات';

  @override
  String get switchLang => 'English';

  @override
  String comingSoonStep(String step) {
    return 'تُبنى في الخطوة $step';
  }

  @override
  String get loadError => 'تعذّر التحميل — تحقق من الاتصال';

  @override
  String get retry => 'إعادة المحاولة';

  @override
  String get lastVerified => 'آخر تحقق:';

  @override
  String get requiredDocuments => 'المتطلبات';

  @override
  String get fees => 'الرسوم';

  @override
  String get location => 'المكان';

  @override
  String get steps => 'الخطوات';

  @override
  String get flagOutdated => 'هل المعلومات قديمة؟ أبلغنا';

  @override
  String get flagThanks => 'شكرًا لك — سيراجع المشرف هذا الدليل.';

  @override
  String get signIn => 'تسجيل الدخول';

  @override
  String get signOut => 'تسجيل الخروج';

  @override
  String get documentsSignIn => 'سجّل الدخول لعرض وثائقك — وثائقك خاصة بحسابك.';

  @override
  String get documentsEmpty => 'لا توجد وثائق بعد.';

  @override
  String get noExpiry => 'بدون انتهاء';

  @override
  String get expired => 'منتهية';

  @override
  String expiresInDays(int days) {
    return 'خلال $days يومًا';
  }

  @override
  String get issueDate => 'تاريخ الإصدار';

  @override
  String get expiryDate => 'تاريخ الانتهاء';

  @override
  String get notes => 'ملاحظات';

  @override
  String get attachments => 'المرفقات';

  @override
  String get noAttachments => 'لا مرفقات.';

  @override
  String get addDocument => 'إضافة وثيقة';

  @override
  String get typeLabel => 'نوع الوثيقة';

  @override
  String get numberLabel => 'رقم الوثيقة (اختياري)';

  @override
  String get pickExpiry => 'تاريخ الانتهاء (اختياري)';

  @override
  String get takePhoto => 'التقاط صورة';

  @override
  String get fromGallery => 'من المعرض';

  @override
  String get photoAttached => 'الصورة جاهزة للرفع';

  @override
  String get save => 'حفظ';

  @override
  String get saveError => 'تعذّر الحفظ — حاول مجددًا.';
}
