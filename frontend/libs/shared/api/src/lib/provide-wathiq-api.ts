import { EnvironmentProviders, InjectionToken, inject, makeEnvironmentProviders } from '@angular/core';

/// '' by default: in dev the Nx serve proxy forwards /api/* to the backend (no CORS at all);
/// a deployed portal provides the real origin here. The URL is configuration, never a literal
/// in a component.
export const WATHIQ_API_BASE_URL = new InjectionToken<string>('WATHIQ_API_BASE_URL', {
  factory: () => '',
});

export function provideWathiqApi(options?: { baseUrl?: string }): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: WATHIQ_API_BASE_URL, useValue: options?.baseUrl ?? '' },
  ]);
}

/// Injection-context helper for httpResource URLs: apiUrl('/api/documents/document-types').
export function injectApiUrl(): (path: string) => string {
  const base = inject(WATHIQ_API_BASE_URL).replace(/\/$/, '');
  return (path: string) => `${base}${path.startsWith('/') ? path : `/${path}`}`;
}
