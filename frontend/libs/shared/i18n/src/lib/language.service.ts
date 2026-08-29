import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

export type AppLang = 'ar' | 'en';

const STORAGE_KEY = 'wathiq.lang';

/// The single source of truth for language. Everything else - direction, locale, <html>
/// attributes, Transloco's active lang - DERIVES from the one `lang` signal, so nothing can
/// disagree with anything (the ValidityPeriod idea applied to UI state).
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly transloco = inject(TranslocoService);
  private readonly document = inject(DOCUMENT);

  readonly lang = signal<AppLang>(readInitialLang());
  readonly dir = computed(() => (this.lang() === 'ar' ? 'rtl' : 'ltr'));
  /// For date/number pipes: pass `lang.locale()` as the pipe's locale ARGUMENT - a pure pipe
  /// re-runs when an argument changes, and reading the signal in the template lets zoneless CD
  /// see the change. (Locale data for both is registered in provideWathiqI18n.)
  readonly locale = computed(() => this.lang());

  constructor() {
    // One effect at the edge of the world: signals -> DOM/storage/Transloco. This is the rare
    // legitimate `effect` - synchronizing state OUT of Angular, not deriving state (checkpoint!).
    effect(() => {
      const lang = this.lang();
      this.transloco.setActiveLang(lang);
      this.document.documentElement.lang = lang;
      this.document.documentElement.dir = this.dir();
      try {
        localStorage.setItem(STORAGE_KEY, lang);
      } catch {
        // storage can be unavailable (private mode) - language still works for the session
      }
    });
  }

  use(lang: AppLang): void {
    this.lang.set(lang);
  }

  toggle(): void {
    this.lang.set(this.lang() === 'ar' ? 'en' : 'ar');
  }
}

function readInitialLang(): AppLang {
  try {
    return localStorage.getItem(STORAGE_KEY) === 'en' ? 'en' : 'ar'; // Arabic-first default
  } catch {
    return 'ar';
  }
}
