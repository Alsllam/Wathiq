import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../auth/auth_notifier.dart';
import '../auth/token_store.dart';
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
  final dio = Dio(
    BaseOptions(
      baseUrl: ref.watch(apiBaseUrlProvider),
      headers: {'Accept-Language': ref.watch(localeProvider).languageCode},
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 30),
    ),
  );

  // Bearer + one refresh-retry (6.6): the authInterceptor idiom in Dio form.
  // ref.read inside the callbacks (not watch): interceptors are ACTIONS - a
  // token change must not rebuild the client mid-request.
  dio.interceptors.add(InterceptorsWrapper(
    onRequest: (options, handler) async {
      final tokens = await ref.read(tokenStoreProvider).read();
      if (tokens != null) {
        options.headers['Authorization'] = 'Bearer ${tokens.accessToken}';
      }
      handler.next(options);
    },
    onError: (error, handler) async {
      final is401 = error.response?.statusCode == 401;
      final alreadyRetried = error.requestOptions.extra['wathiq.retried'] == true;
      if (!is401 || alreadyRetried) {
        return handler.next(error);
      }
      // One retry, flagged on the request - never a refresh storm.
      final newAccess =
          await ref.read(authProvider.notifier).refreshAccessToken();
      if (newAccess == null) {
        return handler.next(error); // signed out; the UI already knows
      }
      final retried = error.requestOptions
        ..extra['wathiq.retried'] = true
        ..headers['Authorization'] = 'Bearer $newAccess';
      try {
        handler.resolve(await dio.fetch<dynamic>(retried));
      } on DioException catch (e) {
        handler.next(e);
      }
    },
  ));

  return dio;
});
