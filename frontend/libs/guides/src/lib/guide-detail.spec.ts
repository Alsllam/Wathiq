import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeAr from '@angular/common/locales/ar';

registerLocaleData(localeAr);   // DatePipe with the ar locale (the 4.8 lesson)
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { GuideDetail } from './guide-detail';

const translations = {
  guides: {
    back: 'عودة', lastVerified: 'آخر تحقق:', requiredDocuments: 'المتطلبات', fees: 'الرسوم',
    location: 'المكان', steps: 'الخطوات', flagOutdated: 'هل المعلومات قديمة؟', flagThanks: 'شكرًا لك',
  },
  documents: { loading: '…', error: 'خطأ' },
};

const detail = {
  id: 'g1',
  slug: 'renew-passport',
  titleAr: 'تجديد جواز السفر',
  titleEn: 'Renew a passport',
  version: {
    id: 'v1', guideId: 'g1', versionNo: 1, language: 'ar',
    bodyMarkdown: '## قبل أن تبدأ\nالتجديد إلكتروني بالكامل.',
    requiredDocuments: 'الهوية الوطنية', fees: '300 ريال', location: 'أبشر',
    lastVerifiedAt: '2026-09-01', publishedAt: '2026-09-03T11:49:26Z',
    steps: ['سدّد الرسوم', 'قدّم الطلب'],
  },
};

describe('GuideDetail', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        GuideDetail,
        TranslocoTestingModule.forRoot({
          langs: { ar: translations },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [provideZonelessChangeDetection(), provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  async function render() {
    const fixture = TestBed.createComponent(GuideDetail);
    fixture.componentRef.setInput('slug', 'renew-passport');
    fixture.detectChanges();
    http.expectOne((r) => r.url.includes('/api/guides/guide/by-slug') && r.url.includes('slug=renew-passport'))
      .flush(detail);
    await fixture.whenStable();
    return fixture;
  }

  it('renders freshness first, the facts card, parsed body and numbered steps', async () => {
    const fixture = await render();
    const el = fixture.nativeElement as HTMLElement;

    expect(el.querySelector('[data-testid="freshness"]')?.textContent).toContain('آخر تحقق:');
    expect(el.textContent).toContain('300 ريال');            // facts card
    expect(el.querySelector('h3')?.textContent).toContain('قبل أن تبدأ');   // parsed heading, no innerHTML
    expect(el.textContent).toContain('قدّم الطلب');           // steps
  });

  it('the outdated button posts anonymous feedback and thanks the reader', async () => {
    const fixture = await render();

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="flag-outdated"]')!.click();
    const post = http.expectOne('/api/guides/guide-feedback');
    expect(post.request.body).toEqual({ guideVersionId: 'v1', kind: 0 });   // Kind 0 = Outdated
    post.flush(null);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="flag-thanks"]')).toBeTruthy();
  });
});
