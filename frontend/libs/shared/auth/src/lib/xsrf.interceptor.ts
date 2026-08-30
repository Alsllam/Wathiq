import { HttpInterceptorFn } from '@angular/common/http';

/// ABP antiforgery for SPAs: the OpenIddict login leaves an auth cookie on `localhost`, and
/// cookies ignore ports - so proxied API POSTs arrive cookie-authenticated and ABP's
/// auto-validation demands the antiforgery header (400 with an empty body otherwise, even with
/// a valid Bearer). ABP issues the token as the XSRF-TOKEN cookie and expects it back as
/// RequestVerificationToken - the exact pairing ABP's own Angular package ships.
export const xsrfInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.method === 'GET' || req.method === 'HEAD') {
    return next(req); // antiforgery guards unsafe verbs only
  }

  const token = document.cookie
    .split('; ')
    .find((c) => c.startsWith('XSRF-TOKEN='))
    ?.substring('XSRF-TOKEN='.length);

  return token
    ? next(req.clone({ setHeaders: { RequestVerificationToken: decodeURIComponent(token) } }))
    : next(req);
};
