import { registerLocaleData } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import localeAr from '@angular/common/locales/ar';
import localeEn from '@angular/common/locales/en';
import { EnvironmentProviders, inject, Injectable, makeEnvironmentProviders } from '@angular/core';
import { provideTransloco, Translation, TranslocoLoader } from '@jsverse/transloco';

/// Lazy per-language JSON over HTTP: only the active language's file is ever downloaded, and a
/// new key lands by editing JSON - no rebuild of the lib.
@Injectable({ providedIn: 'root' })
export class WathiqTranslocoLoader implements TranslocoLoader {
  private readonly http = inject(HttpClient);

  getTranslation(lang: string) {
    return this.http.get<Translation>(`/i18n/${lang}.json`);
  }
}

export function provideWathiqI18n(options?: { prodMode?: boolean }): EnvironmentProviders {
  // DatePipe/DecimalPipe throw on an unregistered locale - both are registered up front so
  // `| date:'':undefined:lang.locale()` works whichever language is active.
  registerLocaleData(localeAr);
  registerLocaleData(localeEn);

  return makeEnvironmentProviders([
    provideTransloco({
      config: {
        availableLangs: ['ar', 'en'],
        defaultLang: 'ar', // Arabic-first (CLAUDE.md); LanguageService may switch immediately
        fallbackLang: 'ar',
        reRenderOnLangChange: true, // translated views re-render on switch - no reload
        prodMode: options?.prodMode ?? false,
      },
      loader: WathiqTranslocoLoader,
    }),
  ]);
}
