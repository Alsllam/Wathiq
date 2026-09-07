import 'package:flutter_appauth/flutter_appauth.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../api/api_client.dart';
import 'auth_tokens.dart';

/// The model-wall pattern (3.3), auth edition: consumers say WHAT they need
/// (sign in, refresh); only the impl knows a browser and a platform channel
/// are involved. Container tests fake this; the device runs the real dance.
abstract class Authorizer {
  Future<AuthTokens?> signIn();
  Future<AuthTokens?> refresh(String refreshToken);
}

class AppAuthAuthorizer implements Authorizer {
  AppAuthAuthorizer({required this.issuer});

  static const _appAuth = FlutterAppAuth();

  /// MUST match discovery byte-exactly - the 4.4 trailing-slash lesson.
  final String issuer;

  /// Registered in AndroidManifest (manifestPlaceholders) and Info.plist:
  /// the OS routes this scheme back into the app after the browser consents.
  static const _redirectUrl = 'sa.wathiq.wathiqmobile://oauthredirect';
  static const _clientId = 'Wathiq_App';
  static const _scopes = ['openid', 'profile', 'offline_access', 'Wathiq'];

  @override
  Future<AuthTokens?> signIn() async {
    // One call = the whole code+PKCE dance: browser out, consent, redirect
    // back, code-for-token exchange. What 4.4 wired by hand, the plugin owns.
    final result = await _appAuth.authorizeAndExchangeCode(
      AuthorizationTokenRequest(
        _clientId,
        _redirectUrl,
        issuer: issuer,
        scopes: _scopes,
      ),
    );
    return _toTokens(result);
  }

  @override
  Future<AuthTokens?> refresh(String refreshToken) async {
    final result = await _appAuth.token(TokenRequest(
      _clientId,
      _redirectUrl,
      issuer: issuer,
      refreshToken: refreshToken,
      scopes: _scopes,
    ));
    return _toTokens(result);
  }

  AuthTokens? _toTokens(TokenResponse result) {
    final access = result.accessToken;
    if (access == null) {
      return null;
    }
    return AuthTokens(
      accessToken: access,
      refreshToken: result.refreshToken,
      expiresAt: result.accessTokenExpirationDateTime,
    );
  }
}

final authorizerProvider = Provider<Authorizer>(
  // The issuer is the API host (OpenIddict lives in the same ABP host) - and
  // the trailing slash matches discovery, as 4.4 taught the hard way.
  (ref) => AppAuthAuthorizer(issuer: '${ref.watch(apiBaseUrlProvider)}/'),
);
