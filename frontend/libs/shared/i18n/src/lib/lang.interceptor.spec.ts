import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { LanguageService } from './language.service';
import { langInterceptor } from './lang.interceptor';

describe('langInterceptor', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [
        TranslocoTestingModule.forRoot({
          langs: { ar: {}, en: {} },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withInterceptors([langInterceptor])),
        provideHttpClientTesting(),
      ],
    });
  });

  it('stamps the APP language, following switches', () => {
    const http = TestBed.inject(HttpClient);
    const ctrl = TestBed.inject(HttpTestingController);
    const lang = TestBed.inject(LanguageService);

    http.get('/x').subscribe();
    ctrl.expectOne((r) => r.headers.get('Accept-Language') === 'ar').flush({});

    lang.toggle();   // the ar↔en switch must reach the SERVER's localizer too
    http.get('/x').subscribe();
    ctrl.expectOne((r) => r.headers.get('Accept-Language') === 'en').flush({});
  });
});
