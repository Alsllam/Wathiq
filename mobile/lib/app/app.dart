import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import '../l10n/gen/app_localizations.dart';
import 'home_shell.dart';

/// The root. Now a StatefulWidget because the locale is the app's first piece
/// of MUTABLE state (a preview - StatefulWidget gets its full treatment in
/// 6.7; here it is three lines: a field, setState, done).
class WathiqApp extends StatefulWidget {
  const WathiqApp({super.key});

  @override
  State<WathiqApp> createState() => _WathiqAppState();
}

class _WathiqAppState extends State<WathiqApp> {
  Locale _locale = const Locale('ar'); // Arabic-first (the 4.2 rule, third stack)

  void _toggleLocale() {
    setState(() {
      _locale = _locale.languageCode == 'ar'
          ? const Locale('en')
          : const Locale('ar');
    });
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      onGenerateTitle: (context) => AppLocalizations.of(context).appName,
      // THE BuildContext lesson: everything below can call Theme.of(context),
      // AppLocalizations.of(context), Directionality.of(context) - each walks
      // UP the tree to what MaterialApp installs right here. Locale 'ar' is
      // also what flips the whole app RTL: no manual mirroring anywhere.
      locale: _locale,
      supportedLocales: const [Locale('ar'), Locale('en')],
      localizationsDelegates: const [
        AppLocalizations.delegate,
        GlobalMaterialLocalizations.delegate, // built-in widget texts in ar
        GlobalWidgetsLocalizations.delegate, // sets Directionality from locale
        GlobalCupertinoLocalizations.delegate,
      ],
      // One seed color = a full Material 3 scheme; 0xFF059669 is the portal's
      // emerald-600, so the two apps read as one product.
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF059669)),
        useMaterial3: true,
      ),
      home: HomeShell(onToggleLocale: _toggleLocale),
    );
  }
}
