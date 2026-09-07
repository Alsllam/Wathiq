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
}
