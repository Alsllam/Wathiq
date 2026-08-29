import { EnvironmentProviders, inject, makeEnvironmentProviders, provideAppInitializer } from '@angular/core';
import { AuthConfig, provideOAuthClient } from 'angular-oauth2-oidc';
import { AuthService } from './auth.service';

export interface WathiqAuthOptions {
  /// The OpenIddict host, e.g. https://localhost:44352 - CORS must allow the SPA origin,
  /// because the code-for-token exchange is a cross-origin POST from the browser.
  issuer: string;
  clientId?: string;
}

export function provideWathiqAuth(options: WathiqAuthOptions): EnvironmentProviders {
  const config: AuthConfig = {
    issuer: options.issuer,
    clientId: options.clientId ?? 'Wathiq_App',
    // No client secret anywhere: PKCE mints a per-login proof instead (responseType 'code'
    // enables PKCE by default in this library).
    responseType: 'code',
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    scope: 'openid profile email roles Wathiq',
    // Dev against self-signed https - strictDiscoveryDocumentValidation trips on ABP's
    // multi-endpoint discovery doc, a known pairing.
    strictDiscoveryDocumentValidation: false,
  };

  return makeEnvironmentProviders([
    provideOAuthClient(),
    { provide: AuthConfig, useValue: config },
    // Runs before the app renders: consumes ?code= when returning from the login page, so
    // guarded routes see the final auth state, never the intermediate one.
    provideAppInitializer(() => inject(AuthService).init()),
  ]);
}
