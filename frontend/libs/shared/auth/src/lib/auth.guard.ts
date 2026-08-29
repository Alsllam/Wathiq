import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

/// Functional guard: an unauthenticated visit to a guarded route goes straight to the login
/// redirect - after OpenIddict sends the user back, tryLogin restores the session and the
/// route resolves on the next navigation.
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  if (auth.isAuthenticated()) {
    return true;
  }

  auth.login(); // full-page redirect; returning false stops the current navigation
  return false;
};
