import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { DocumentsList } from './documents-list';

const translations = {
  documents: {
    title: 'وثائقي', loading: '…', error: 'خطأ', empty: 'لا توجد وثائق بعد.',
    expiry: { expired: 'منتهية منذ {{days}} يوم', soon: 'تنتهي خلال {{days}} يوم', ok: 'سارية', none: 'بلا تاريخ' },
  },
};

describe('DocumentsList', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [
        DocumentsList,
        TranslocoTestingModule.forRoot({
          langs: { ar: translations },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  function flushBoth(fixture: { detectChanges(): void }, docs: object[], total = docs.length) {
    fixture.detectChanges(); // resources issue their requests (the 4.3 ordering lesson)
    http.expectOne((r) => r.url.includes('/api/documents/documents?')).flush({ items: docs, totalCount: total });
    http.expectOne('/api/documents/document-types').flush({
      items: [{ id: 't1', code: 'PASSPORT', nameAr: 'جواز السفر', nameEn: 'Passport' }],
    });
  }

  it('joins type names and derives expiry chips per row', async () => {
    const fixture = TestBed.createComponent(DocumentsList);
    flushBoth(fixture, [
      { id: 'd1', documentTypeId: 't1', number: 'P-1', daysUntilExpiry: 7 },
      { id: 'd2', documentTypeId: 't1', number: 'P-2', daysUntilExpiry: -3 },
    ]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('جواز السفر');          // catalogue join, Arabic column
    expect(text).toContain('تنتهي خلال 7 يوم');    // soon chip with the day count
    expect(text).toContain('منتهية منذ 3 يوم');    // expired chip uses ABS days
  });

  it('paging rewrites the resource URL - the click IS the refetch', async () => {
    const fixture = TestBed.createComponent(DocumentsList);
    flushBoth(fixture, [{ id: 'd1', documentTypeId: 't1', daysUntilExpiry: null }], 25);
    await fixture.whenStable();

    (fixture.nativeElement as HTMLElement)
      .querySelectorAll<HTMLButtonElement>('nav button')[1].click();
    fixture.detectChanges();

    const second = http.expectOne((r) => r.url.includes('SkipCount=10'));
    second.flush({ items: [], totalCount: 25 });
    await fixture.whenStable();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('لا توجد وثائق بعد.');
  });
});
