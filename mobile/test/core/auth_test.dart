import 'dart:convert';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/core/auth/auth_notifier.dart';
import 'package:wathiq_mobile/core/auth/auth_tokens.dart';
import 'package:wathiq_mobile/core/auth/authorizer.dart';
import 'package:wathiq_mobile/core/auth/token_store.dart';

/// A JWT with only the parts that matter: header.payload.signature, payload
/// carrying preferred_username - built here so the test owns its fixture.
String fakeJwt(Map<String, dynamic> claims) {
  String b64(Object o) => base64Url.encode(utf8.encode(jsonEncode(o)));
  return '${b64({'alg': 'none'})}.${b64(claims)}.sig';
}

class InMemoryTokenStore implements TokenStore {
  AuthTokens? tokens;
  @override
  Future<AuthTokens?> read() async => tokens;
  @override
  Future<void> save(AuthTokens value) async => tokens = value;
  @override
  Future<void> clear() async => tokens = null;
}

class FakeAuthorizer implements Authorizer {
  AuthTokens? nextSignIn;
  AuthTokens? nextRefresh;
  final refreshedWith = <String>[];

  @override
  Future<AuthTokens?> signIn() async => nextSignIn;
  @override
  Future<AuthTokens?> refresh(String refreshToken) async {
    refreshedWith.add(refreshToken);
    return nextRefresh;
  }
}

void main() {
  group('decodeJwtClaims', () {
    test('reads claims from a well-formed token, torn ones yield nothing', () {
      expect(userNameFromToken(fakeJwt({'preferred_username': 'admin'})),
          'admin');
      expect(decodeJwtClaims('not-a-jwt'), isEmpty);
      expect(decodeJwtClaims('a.%%%.c'), isEmpty); // garbage payload → {} not throw
    });
  });

  group('AuthNotifier', () {
    late InMemoryTokenStore store;
    late FakeAuthorizer authorizer;
    late ProviderContainer container;

    setUp(() {
      store = InMemoryTokenStore();
      authorizer = FakeAuthorizer();
      container = ProviderContainer(overrides: [
        tokenStoreProvider.overrideWithValue(store),
        authorizerProvider.overrideWithValue(authorizer),
      ]);
      addTearDown(container.dispose);
    });

    test('hydrates SignedIn from a stored token on launch', () async {
      store.tokens =
          AuthTokens(accessToken: fakeJwt({'preferred_username': 'admin'}));

      final state = await container.read(authProvider.future);

      expect(state, isA<SignedIn>().having((s) => s.userName, 'name', 'admin'));
    });

    test('signIn stores tokens; cancelled browser changes nothing', () async {
      await container.read(authProvider.future); // hydrate SignedOut

      await container.read(authProvider.notifier).signIn(); // nextSignIn null
      expect(container.read(authProvider).value, isA<SignedOut>());

      authorizer.nextSignIn =
          AuthTokens(accessToken: fakeJwt({'preferred_username': 'amina'}));
      await container.read(authProvider.notifier).signIn();

      expect(store.tokens, isNotNull); // persisted for the next launch
      expect((container.read(authProvider).value! as SignedIn).userName,
          'amina');
    });

    test('refresh renews and persists; a dead refresh token signs out',
        () async {
      store.tokens = AuthTokens(
          accessToken: fakeJwt({'preferred_username': 'admin'}),
          refreshToken: 'r1');
      await container.read(authProvider.future);

      authorizer.nextRefresh = AuthTokens(
          accessToken: fakeJwt({'preferred_username': 'admin'}),
          refreshToken: 'r2');
      final renewed =
          await container.read(authProvider.notifier).refreshAccessToken();

      expect(renewed, isNotNull);
      expect(authorizer.refreshedWith, ['r1']);
      expect(store.tokens!.refreshToken, 'r2'); // rotation persisted

      // Server rejects the refresh → tokens cleared, state SignedOut: the UI
      // flips to the sign-in icon without any screen doing 401 bookkeeping.
      authorizer.nextRefresh = null;
      final failed =
          await container.read(authProvider.notifier).refreshAccessToken();
      expect(failed, isNull);
      expect(store.tokens, isNull);
      expect(container.read(authProvider).value, isA<SignedOut>());
    });

    test('signOut clears the store', () async {
      store.tokens = AuthTokens(accessToken: fakeJwt({'sub': 'x'}));
      await container.read(authProvider.future);

      await container.read(authProvider.notifier).signOut();

      expect(store.tokens, isNull);
      expect(container.read(authProvider).value, isA<SignedOut>());
    });
  });
}
