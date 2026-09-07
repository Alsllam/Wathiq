import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../locale_provider.dart';

/// Where the backend lives. Overridable at build time
/// (`--dart-define=WATHIQ_API_URL=…`); otherwise a platform default - the
/// Android EMULATOR reaches the host machine at 10.0.2.2, not localhost
/// (localhost inside the emulator is the phone itself).
final apiBaseUrlProvider = Provider<String>((ref) {
  const fromEnv = String.fromEnvironment('WATHIQ_API_URL');
  if (fromEnv.isNotEmpty) {
    return fromEnv;
  }
  return defaultTargetPlatform == TargetPlatform.android
      ? 'https://10.0.2.2:44352'
      : 'https://localhost:44352';
});

/// The one HTTP client. `ref.watch(localeProvider)` is a DEPENDENCY EDGE, the
/// same declaration a computed() makes: switch the language and this provider
/// rebuilds with the new Accept-Language - so server-localized strings follow
/// the app language by construction (5.7's live-caught bug, prevented here on
/// day one), and everything watching the client refetches in the new language.
final dioProvider = Provider<Dio>((ref) {
  return Dio(
    BaseOptions(
      baseUrl: ref.watch(apiBaseUrlProvider),
      headers: {'Accept-Language': ref.watch(localeProvider).languageCode},
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 30),
    ),
  );
});
