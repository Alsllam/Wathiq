import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

/// 6.3's setState preview graduates into app state proper: ONE writable source
/// of truth for the locale, watched by the app root (rebuilds MaterialApp) AND
/// by the HTTP client (rewrites Accept-Language). A Notifier is the writable
/// signal: `build()` is the initial value, methods are the only mutations.
class LocaleNotifier extends Notifier<Locale> {
  @override
  Locale build() => const Locale('ar'); // Arabic-first, as everywhere

  void toggle() {
    state = state.languageCode == 'ar' ? const Locale('en') : const Locale('ar');
  }
}

final localeProvider = NotifierProvider<LocaleNotifier, Locale>(
  LocaleNotifier.new,
);
