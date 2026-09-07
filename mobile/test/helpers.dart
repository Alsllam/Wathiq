import 'dart:convert';

import 'package:wathiq_mobile/core/auth/auth_tokens.dart';
import 'package:wathiq_mobile/core/auth/authorizer.dart';
import 'package:wathiq_mobile/core/auth/token_store.dart';

/// A JWT with only the parts that matter: header.payload.signature - built
/// here so tests own their fixture.
String fakeJwt(Map<String, dynamic> claims) {
  String b64(Object o) => base64Url.encode(utf8.encode(jsonEncode(o)));
  return '${b64({'alg': 'none'})}.${b64(claims)}.sig';
}

class InMemoryTokenStore implements TokenStore {
  InMemoryTokenStore([this.tokens]);
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

/// A store already holding a signed-in admin - the one-liner most widget
/// tests want instead of relying on MissingPluginException degradation (6.6).
InMemoryTokenStore signedInStore({String userName = 'admin'}) =>
    InMemoryTokenStore(
        AuthTokens(accessToken: fakeJwt({'preferred_username': userName})));
