import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { appRoutes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Zoneless from day one (4.1): no zone.js patching - change detection runs off signals and
    // events, so every piece of state MUST live in a signal to be seen. Retrofitting this later
    // means auditing every mutation; starting here makes the discipline free.
    provideZonelessChangeDetection(),
    provideRouter(appRoutes),
  ],
};
