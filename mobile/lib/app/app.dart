import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/locale_provider.dart';
import '../l10n/gen/app_localizations.dart';
import 'home_shell.dart';

/// Back to stateless - but a ConsumerWidget: the locale moved OUT of the tree
/// into localeProvider (6.4), and this root simply watches it. State that two
/// unrelated consumers need (MaterialApp here, the Dio client in core/) never
/// belongs to a widget.
class WathiqApp extends ConsumerWidget {
  const WathiqApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return MaterialApp(
      onGenerateTitle: (context) => AppLocalizations.of(context).appName,
      // THE BuildContext lesson (6.3): everything below can call Theme.of,
      // AppLocalizations.of, Directionality.of - each walks UP the tree to
      // what MaterialApp installs right here. Locale 'ar' also flips the
      // whole app RTL: no manual mirroring anywhere.
      locale: ref.watch(localeProvider),
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
      home: const HomeShell(),
    );
  }
}
