import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeAr from '@angular/common/locales/ar';

// Production registers this inside provideWathiqI18n; the spec builds its own injector and the
// DatePipe throws NG0701 on an unregistered locale (idempotent, so module scope is fine).
registerLocaleData(localeAr);
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { RemindersPage } from './reminders-page';

const translations = {
  reminders: {
    title: 'الجدول', empty: 'لا شيء', overdueBy: 'متأخر {{days}}', inDays: 'بعد {{days}} يوم',
    settings: 'الإعدادات', offsets: 'قبل', days: '{{days}} يوم', remove: 'إزالة', channels: 'قنوات',
    email: 'بريد', push: 'جوال', soon: 'قريبًا', quiet: 'هدوء', quietPair: 'معًا', timezone: 'المنطقة',
    save: 'حفظ', saved: 'تم',
  },
  documents: { loading: '…', error: 'خطأ' },
};

describe('RemindersPage', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [
        RemindersPage,
        TranslocoTestingModule.forRoot({
          langs: { ar: translations },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [provideZonelessChangeDetection(), provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  function flushAll(fixture: { detectChanges(): void }) {
    fixture.detectChanges();
    http.expectOne('/api/reminders/reminders/upcoming-list').flush({
      items: [{ id: 'r1', documentId: 'd1', offsetDays: 30, dueDate: '2036-02-01' }],
    });
    http.expectOne('/api/reminders/rule').flush({
      offsetsDays: [90, 30], channels: 1, quietFrom: null, quietTo: null, timeZoneId: 'Asia/Riyadh',
    });
    http.expectOne((r) => r.url.startsWith('/api/documents/documents')).flush({
      items: [{ id: 'd1', number: 'P-102030' }], totalCount: 1,
    });
  }

  it('joins the timeline rows to document numbers and seeds the editor once', async () => {
    const fixture = TestBed.createComponent(RemindersPage);
    flushAll(fixture);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('P-102030');
    expect(fixture.componentInstance['state'].offsets()).toEqual([90, 30]);
  });

  it('save PUTs the edited rule and reloads the timeline', async () => {
    const fixture = TestBed.createComponent(RemindersPage);
    flushAll(fixture);
    await fixture.whenStable();

    const state = fixture.componentInstance['state'];
    state.newOffset.set('7');
    state.addOffset();
    fixture.componentInstance['save']();

    const put = http.expectOne('/api/reminders/rule');
    expect(put.request.method).toBe('PUT');
    expect(put.request.body.offsetsDays).toEqual([90, 30, 7]);
    put.flush({});
    fixture.detectChanges(); // reload() issues its request via effects (the 4.3 ordering lesson)
    // The reload the save triggers: the rule change resynced reminders server-side.
    http.expectOne('/api/reminders/reminders/upcoming-list').flush({ items: [] });
    await fixture.whenStable();
    expect(fixture.componentInstance['saved']()).toBe(true);
  });
});
