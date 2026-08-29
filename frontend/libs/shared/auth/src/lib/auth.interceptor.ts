import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

/// Functional interceptor (the class + DI-token ceremony is gone): attach the bearer token to
/// API calls only - the i18n loader, discovery document and third parties stay untouched.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).accessToken();
  const isApiCall = req.url.startsWith('/api') || req.url.includes('/api/');

  return token && isApiCall
    ? next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))
    : next(req);
};
