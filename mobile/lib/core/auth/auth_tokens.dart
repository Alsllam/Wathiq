import 'dart:convert';

/// What a successful sign-in/refresh yields. Refresh token nullable: the
/// server may not rotate it on refresh.
class AuthTokens {
  const AuthTokens({
    required this.accessToken,
    this.refreshToken,
    this.expiresAt,
  });

  final String accessToken;
  final String? refreshToken;
  final DateTime? expiresAt;
}

/// Reads the JWT payload WITHOUT verifying it - fine here because the token
/// came over TLS from OUR issuer and is only used for display (userName);
/// the backend re-validates the signature on every request, where it matters.
/// Pure Dart (base64Url + json), so it's fully testable in the container.
Map<String, dynamic> decodeJwtClaims(String jwt) {
  final parts = jwt.split('.');
  if (parts.length != 3) {
    return const {};
  }
  try {
    // base64Url without padding is legal in JWTs; normalize adds it back.
    final payload = utf8.decode(base64Url.decode(base64Url.normalize(parts[1])));
    return (jsonDecode(payload) as Map<String, dynamic>?) ?? const {};
  } on FormatException {
    return const {}; // a torn token is "no claims", not a crash (3.6 posture)
  }
}

/// OpenIddict puts the username in preferred_username (and ABP mirrors it).
String? userNameFromToken(String accessToken) {
  final claims = decodeJwtClaims(accessToken);
  return (claims['preferred_username'] ?? claims['unique_name'] ?? claims['sub'])
      as String?;
}
