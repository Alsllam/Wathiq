import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { GuidesList } from './guides-list';

const translations = {
  guides: { title: 'أدلة التجديد', subtitle: 'أدلة', empty: 'لا توجد', askButton: 'اسأل المساعد' },
  documents: { loading: '…', error: 'خطأ' },
};

describe('GuidesList', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        GuidesList,
        TranslocoTestingModule.forRoot({
          langs: { ar: translations },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [provideZonelessChangeDetection(), provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  it('renders the public list with language-appropriate titles', async () => {
    const fixture = TestBed.createComponent(GuidesList);
    fixture.detectChanges();
    http.expectOne('/api/guides/guide').flush({
      items: [{ id: '1', slug: 'renew-passport', titleAr: 'تجديد جواز السفر', titleEn: 'Renew a passport' }],
    });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent!;
    expect(text).toContain('تجديد جواز السفر');   // ar default leads
    expect(text).toContain('Renew a passport');   // the other language as the subtitle
    const link = (fixture.nativeElement as HTMLElement).querySelector('li a');
    expect(link?.getAttribute('href')).toContain('renew-passport');
  });
});
