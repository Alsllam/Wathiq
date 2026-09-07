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
}
