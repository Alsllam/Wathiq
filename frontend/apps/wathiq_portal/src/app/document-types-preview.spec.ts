import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { DocumentTypesPreview } from './document-types-preview';

/* eslint-disable @typescript-eslint/no-require-imports */
const ar = require('../../public/i18n/ar.json');

describe('DocumentTypesPreview', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [
        DocumentTypesPreview,
        TranslocoTestingModule.forRoot({
          langs: { ar },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
  });

  it('renders the catalogue from the API response, Arabic names first', async () => {
    const fixture = TestBed.createComponent(DocumentTypesPreview);
    // NOT whenStable() yet: httpResource registers a pending task, so stability waits for the
    // response - which we cannot flush before the request exists. detectChanges() runs the
    // effects that issue the request; whenStable() comes after the flush.
    fixture.detectChanges();

    const http = TestBed.inject(HttpTestingController);
    // The URL the resource computed via injectApiUrl - relative, dev-proxy shaped.
    http.expectOne('/api/documents/document-types').flush({
      items: [
        { id: '1', code: 'PASSPORT', nameAr: 'جواز السفر', nameEn: 'Passport' },
        { id: '2', code: 'NATIONAL_ID', nameAr: 'الهوية الوطنية', nameEn: 'National ID' },
      ],
    });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('جواز السفر');
    expect(text).toContain('الهوية الوطنية');
    http.verify();
  });
});
