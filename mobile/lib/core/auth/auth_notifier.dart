import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'auth_tokens.dart';
import 'authorizer.dart';
import 'token_store.dart';

/// Sealed states (6.5's lesson applied to auth): a screen switching on this
/// must handle both, by compiler order.
sealed class AuthState {
  const AuthState();
}

class SignedOut extends AuthState {
  const SignedOut();
}

class SignedIn extends AuthState {
  const SignedIn({required this.userName});
  final String userName;
}

/// AsyncNotifier = Notifier whose build() is async - made for "hydrate initial
/// state from storage": watchers see `AsyncValue<AuthState>`, the same
/// loading/error/data triple as every provider read.
class AuthNotifier extends AsyncNotifier<AuthState> {
  @override
  Future<AuthState> build() async {
    // Restore-on-launch: a stored token means the LAST session signed in; the
    // API is still the judge (a stale token just 401s into the refresh path).
    final tokens = await ref.read(tokenStoreProvider).read();
    return _toState(tokens);
  }

  Future<void> signIn() async {
    final tokens = await ref.read(authorizerProvider).signIn();
    if (tokens == null) {
      return; // user cancelled the browser - not an error, just no change
    }
    await ref.read(tokenStoreProvider).save(tokens);
    state = AsyncData(_toState(tokens));
  }

  Future<void> signOut() async {
    await ref.read(tokenStoreProvider).clear();
    state = const AsyncData(SignedOut());
  }

  /// The 401 path: exchange the refresh token, persist, return the new access
  /// token (null = re-login required, and the state says so immediately).
  Future<String?> refreshAccessToken() async {
    final store = ref.read(tokenStoreProvider);
    final current = await store.read();
    final refreshToken = current?.refreshToken;
    if (refreshToken == null) {
      return null;
    }
    final renewed = await ref.read(authorizerProvider).refresh(refreshToken);
    if (renewed == null) {
      await store.clear();
      state = const AsyncData(SignedOut());
      return null;
    }
    await store.save(renewed);
    state = AsyncData(_toState(renewed));
    return renewed.accessToken;
  }

  AuthState _toState(AuthTokens? tokens) {
    if (tokens == null) {
      return const SignedOut();
    }
    return SignedIn(userName: userNameFromToken(tokens.accessToken) ?? '');
  }
}

final authProvider =
    AsyncNotifierProvider<AuthNotifier, AuthState>(AuthNotifier.new);
