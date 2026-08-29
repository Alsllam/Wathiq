import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { OAuthService } from 'angular-oauth2-oidc';
import { Subject } from 'rxjs';
import { App } from './app';

// The header reads AuthService, whose only dependency is the OAuth library - stubbed here as
// "anonymous, nothing happening" (the auth lib's own spec covers the interesting states).
const oauthStub = {
  events: new Subject(),
  hasValidAccessToken: () => false,
  getIdentityClaims: () => null,
};

// require, not import: under jest's CJS the ESM-interop wraps json imports in {default}, and
// Transloco would flatten that into "default.shell.appName" keys.
/* eslint-disable @typescript-eslint/no-require-imports */
const ar = require('../../public/i18n/ar.json');
const en = require('../../public/i18n/en.json');

describe('App', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [
        App,
        // The REAL translation files, loaded synchronously - keys drift is caught by the test,
        // and no HTTP loader runs in specs.
        TranslocoTestingModule.forRoot({
          langs: { ar, en },
          // reRenderOnLangChange must MATCH production (provideWathiqI18n sets it): it defaults
          // to false, which would freeze the pipe on the first language forever.
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar', reRenderOnLangChange: true },
        }),
      ],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        // App now embeds DocumentTypesPreview (httpResource) - requests stay stubbed here.
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: OAuthService, useValue: oauthStub },
      ],
    }).compileComponents();
  });

  // The embedded httpResource holds app stability until its request is answered - flush it
  // first (detectChanges issues it), or every whenStable() in these tests times out.
  function flushCatalogue(fixture: { detectChanges(): void }) {
    fixture.detectChanges();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/documents/document-types')
      .flush({ items: [] });
  }

  it('renders the Arabic-first shell header', async () => {
    const fixture = TestBed.createComponent(App);
    flushCatalogue(fixture);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('وثيق');
  });

  it('the language toggle switches the rendered language and direction', async () => {
    const fixture = TestBed.createComponent(App);
    flushCatalogue(fixture);
    await fixture.whenStable();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="lang-toggle"]')?.click();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Wathiq');
    expect(document.documentElement.dir).toBe('ltr');
  });
});
