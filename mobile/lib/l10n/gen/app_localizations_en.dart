// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appName => 'Wathiq';

  @override
  String get tagline => 'Your documents and their deadlines';

  @override
  String get navGuides => 'Guides';

  @override
  String get navDocuments => 'Documents';

  @override
  String get navReminders => 'Reminders';

  @override
  String get switchLang => 'العربية';

  @override
  String comingSoonStep(String step) {
    return 'Built in step $step';
  }

  @override
  String get loadError => 'Could not load — check your connection';

  @override
  String get retry => 'Retry';

  @override
  String get lastVerified => 'Last verified:';

  @override
  String get requiredDocuments => 'Requirements';

  @override
  String get fees => 'Fees';

  @override
  String get location => 'Where';

  @override
  String get steps => 'Steps';

  @override
  String get flagOutdated => 'Outdated? Tell us';

  @override
  String get flagThanks => 'Thank you — an admin will review this guide.';

  @override
  String get signIn => 'Sign in';

  @override
  String get signOut => 'Sign out';

  @override
  String get documentsSignIn =>
      'Sign in to see your documents — they belong to your account.';

  @override
  String get documentsEmpty => 'No documents yet.';

  @override
  String get noExpiry => 'No expiry';

  @override
  String get expired => 'Expired';

  @override
  String expiresInDays(int days) {
    return 'In $days days';
  }

  @override
  String get issueDate => 'Issue date';

  @override
  String get expiryDate => 'Expiry date';

  @override
  String get notes => 'Notes';

  @override
  String get attachments => 'Attachments';

  @override
  String get noAttachments => 'No attachments.';
}
