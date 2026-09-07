import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LanguageService } from './language.service';

/**
 * Server-side localization follows the APP language, not the browser's. Without this, ABP
 * localizes error messages and the chat's refusal text from the browser's Accept-Language -
 * an Arabic UI showing English refusals (caught live in 5.7). One header, every endpoint.
 */
export const langInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = inject(LanguageService).lang();
  return next(req.clone({ setHeaders: { 'Accept-Language': lang } }));
};
