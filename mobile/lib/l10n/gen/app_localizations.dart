import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_ar.dart';
import 'app_localizations_en.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'gen/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('ar'),
    Locale('en'),
  ];

  /// No description provided for @appName.
  ///
  /// In ar, this message translates to:
  /// **'وثيق'**
  String get appName;

  /// No description provided for @tagline.
  ///
  /// In ar, this message translates to:
  /// **'مساعدك لوثائقك ومواعيدها'**
  String get tagline;

  /// No description provided for @navGuides.
  ///
  /// In ar, this message translates to:
  /// **'الأدلة'**
  String get navGuides;

  /// No description provided for @navDocuments.
  ///
  /// In ar, this message translates to:
  /// **'الوثائق'**
  String get navDocuments;

  /// No description provided for @navReminders.
  ///
  /// In ar, this message translates to:
  /// **'التذكيرات'**
  String get navReminders;

  /// No description provided for @switchLang.
  ///
  /// In ar, this message translates to:
  /// **'English'**
  String get switchLang;

  /// No description provided for @comingSoonStep.
  ///
  /// In ar, this message translates to:
  /// **'تُبنى في الخطوة {step}'**
  String comingSoonStep(String step);

  /// No description provided for @loadError.
  ///
  /// In ar, this message translates to:
  /// **'تعذّر التحميل — تحقق من الاتصال'**
  String get loadError;

  /// No description provided for @retry.
  ///
  /// In ar, this message translates to:
  /// **'إعادة المحاولة'**
  String get retry;

  /// No description provided for @lastVerified.
  ///
  /// In ar, this message translates to:
  /// **'آخر تحقق:'**
  String get lastVerified;

  /// No description provided for @requiredDocuments.
  ///
  /// In ar, this message translates to:
  /// **'المتطلبات'**
  String get requiredDocuments;

  /// No description provided for @fees.
  ///
  /// In ar, this message translates to:
  /// **'الرسوم'**
  String get fees;

  /// No description provided for @location.
  ///
  /// In ar, this message translates to:
  /// **'المكان'**
  String get location;

  /// No description provided for @steps.
  ///
  /// In ar, this message translates to:
  /// **'الخطوات'**
  String get steps;

  /// No description provided for @flagOutdated.
  ///
  /// In ar, this message translates to:
  /// **'هل المعلومات قديمة؟ أبلغنا'**
  String get flagOutdated;

  /// No description provided for @flagThanks.
  ///
  /// In ar, this message translates to:
  /// **'شكرًا لك — سيراجع المشرف هذا الدليل.'**
  String get flagThanks;

  /// No description provided for @signIn.
  ///
  /// In ar, this message translates to:
  /// **'تسجيل الدخول'**
  String get signIn;

  /// No description provided for @signOut.
  ///
  /// In ar, this message translates to:
  /// **'تسجيل الخروج'**
  String get signOut;

  /// No description provided for @documentsSignIn.
  ///
  /// In ar, this message translates to:
  /// **'سجّل الدخول لعرض وثائقك — وثائقك خاصة بحسابك.'**
  String get documentsSignIn;

  /// No description provided for @documentsEmpty.
  ///
  /// In ar, this message translates to:
  /// **'لا توجد وثائق بعد.'**
  String get documentsEmpty;

  /// No description provided for @noExpiry.
  ///
  /// In ar, this message translates to:
  /// **'بدون انتهاء'**
  String get noExpiry;

  /// No description provided for @expired.
  ///
  /// In ar, this message translates to:
  /// **'منتهية'**
  String get expired;

  /// No description provided for @expiresInDays.
  ///
  /// In ar, this message translates to:
  /// **'خلال {days} يومًا'**
  String expiresInDays(int days);

  /// No description provided for @issueDate.
  ///
  /// In ar, this message translates to:
  /// **'تاريخ الإصدار'**
  String get issueDate;

  /// No description provided for @expiryDate.
  ///
  /// In ar, this message translates to:
  /// **'تاريخ الانتهاء'**
  String get expiryDate;

  /// No description provided for @notes.
  ///
  /// In ar, this message translates to:
  /// **'ملاحظات'**
  String get notes;

  /// No description provided for @attachments.
  ///
  /// In ar, this message translates to:
  /// **'المرفقات'**
  String get attachments;

  /// No description provided for @noAttachments.
  ///
  /// In ar, this message translates to:
  /// **'لا مرفقات.'**
  String get noAttachments;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['ar', 'en'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'ar':
      return AppLocalizationsAr();
    case 'en':
      return AppLocalizationsEn();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
