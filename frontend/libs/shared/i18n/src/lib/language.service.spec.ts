import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslocoService } from '@jsverse/transloco';
import { LanguageService } from './language.service';
import { provideWathiqI18n } from './provide-wathiq-i18n';

describe('LanguageService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(), // translation fetches stay stubbed - no network in tests
        provideWathiqI18n(),
      ],
    });
  });

  it('is Arabic-first: ar, rtl, and the <html> attributes follow', () => {
    const service = TestBed.inject(LanguageService);
    TestBed.tick(); // run the sync effect (zoneless tests drive the scheduler explicitly)

    expect(service.lang()).toBe('ar');
    expect(service.dir()).toBe('rtl');
    expect(document.documentElement.dir).toBe('rtl');
    expect(document.documentElement.lang).toBe('ar');
  });

  it('toggle flips every derived surface at once', () => {
    const service = TestBed.inject(LanguageService);
    const transloco = TestBed.inject(TranslocoService);

    service.toggle();
    TestBed.tick();

    expect(service.lang()).toBe('en');
    expect(service.dir()).toBe('ltr');
    expect(service.locale()).toBe('en');
    expect(document.documentElement.dir).toBe('ltr');
    expect(transloco.getActiveLang()).toBe('en');
    expect(localStorage.getItem('wathiq.lang')).toBe('en');
  });

  it('remembers the choice across construction', () => {
    localStorage.setItem('wathiq.lang', 'en');
    expect(TestBed.inject(LanguageService).lang()).toBe('en');
  });
});
