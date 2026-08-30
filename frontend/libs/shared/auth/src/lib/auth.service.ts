import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { OAuthService } from 'angular-oauth2-oidc';
import { injectApiUrl } from '@wathiq/shared/api';

/// The signals facade over angular-oauth2-oidc (4.2's recipe again): the library's event
/// stream is the edge; ONE writable signal mirrors it; everything the UI needs derives.
/// Components never see OAuthService.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oauth = inject(OAuthService);
  private readonly http = inject(HttpClient);
  private readonly apiUrl = injectApiUrl();

  private readonly authenticated = signal(false);

  readonly isAuthenticated = this.authenticated.asReadonly();
  /// From ID-token claims - ABP puts the username in preferred_username.
  readonly userName = computed(() => {
    if (!this.authenticated()) {
      return null;
    }
    const claims = this.oauth.getIdentityClaims() as { preferred_username?: string; name?: string } | null;
    return claims?.preferred_username ?? claims?.name ?? null;
  });

  constructor() {
    // Any token lifecycle event re-derives the one source signal - login, silent refresh,
    // logout, expiry all collapse into "is there a valid access token right now?".
    this.oauth.events.subscribe(() => this.authenticated.set(this.oauth.hasValidAccessToken()));
  }

  /// Discovery + "did we just come back with ?code=" - call once at startup (APP_INITIALIZER).
  async init(): Promise<void> {
    await this.oauth.loadDiscoveryDocumentAndTryLogin();
    this.authenticated.set(this.oauth.hasValidAccessToken());
    if (this.authenticated()) {
      // ABP's bootstrap call, for one side effect we need: it RE-ISSUES the XSRF-TOKEN cookie
      // bound to the CURRENT principal. The pre-login anonymous token otherwise fails POSTs
      // with "meant for a different claims-based user" (the antiforgery is per-principal).
      await firstValueFrom(this.http.get(this.apiUrl('/api/abp/application-configuration')))
        .catch(() => undefined); // backend down = degraded, not broken login
    }
  }

  login(): void {
    this.oauth.initCodeFlow(); // full-page redirect to /connect/authorize (code + PKCE)
  }

  logout(): void {
    this.oauth.logOut(); // end_session on the server, then back to postLogoutRedirectUri
  }

  accessToken(): string | null {
    return this.oauth.hasValidAccessToken() ? this.oauth.getAccessToken() : null;
  }
}
