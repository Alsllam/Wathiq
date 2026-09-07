import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'auth_tokens.dart';

/// The seam over storage: the notifier and the Dio interceptor depend on THIS,
/// container tests swap in a Map-backed fake, and only the impl below knows a
/// platform channel exists.
abstract class TokenStore {
  Future<AuthTokens?> read();
  Future<void> save(AuthTokens tokens);
  Future<void> clear();
}

/// flutter_secure_storage = Keychain (iOS) / EncryptedSharedPreferences
/// (Android) behind one Dart facade - a PLUGIN, so it only truly runs on a
/// device; that is why it hides behind the interface.
class SecureTokenStore implements TokenStore {
  const SecureTokenStore(this._storage);

  final FlutterSecureStorage _storage;

  static const _access = 'wathiq.accessToken';
  static const _refresh = 'wathiq.refreshToken';
  static const _expiry = 'wathiq.expiresAt';

  @override
  Future<AuthTokens?> read() async {
    final access = await _storage.read(key: _access);
    if (access == null) {
      return null;
    }
    final expiryRaw = await _storage.read(key: _expiry);
    return AuthTokens(
      accessToken: access,
      refreshToken: await _storage.read(key: _refresh),
      expiresAt: expiryRaw == null ? null : DateTime.tryParse(expiryRaw),
    );
  }

  @override
  Future<void> save(AuthTokens tokens) async {
    await _storage.write(key: _access, value: tokens.accessToken);
    if (tokens.refreshToken case final refresh?) {
      await _storage.write(key: _refresh, value: refresh);
    }
    if (tokens.expiresAt case final expiry?) {
      await _storage.write(key: _expiry, value: expiry.toIso8601String());
    }
  }

  @override
  Future<void> clear() async {
    await _storage.delete(key: _access);
    await _storage.delete(key: _refresh);
    await _storage.delete(key: _expiry);
  }
}

final tokenStoreProvider = Provider<TokenStore>(
  (ref) => const SecureTokenStore(FlutterSecureStorage()),
);
