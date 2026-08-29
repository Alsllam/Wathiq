import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { authInterceptor, provideWathiqAuth } from '@wathiq/shared/auth';
import { provideWathiqI18n } from '@wathiq/shared/i18n';
import { appRoutes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Zoneless from day one (4.1): no zone.js patching - change detection runs off signals and
    // events, so every piece of state MUST live in a signal to be seen. Retrofitting this later
    // means auditing every mutation; starting here makes the discipline free.
    provideZonelessChangeDetection(),
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideWathiqI18n(), // ar-first Transloco + registered ar/en locale data (4.2)
    // Issuer is absolute (OIDC discovery returns absolute endpoints anyway); API calls stay
    // relative through the dev proxy. CORS on the host allows this origin for the token POST.
    provideWathiqAuth({ issuer: 'https://localhost:44352/' }), // trailing slash = OpenIddict's exact issuer
  ],
};
