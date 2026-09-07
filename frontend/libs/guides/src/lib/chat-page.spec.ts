import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeAr from '@angular/common/locales/ar';

registerLocaleData(localeAr);
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { AuthService } from '@wathiq/shared/auth';
import { ChatPage } from './chat-page';

const translations = {
  guides: {
    back: 'عودة', chatTitle: 'مساعد الأدلة', chatSubtitle: 'من الأدلة فقط', chatSignIn: 'سجّل الدخول',
    askPlaceholder: 'اسأل', send: 'إرسال', thinking: 'يبحث…', sources: 'المصادر',
    lastVerified: 'آخر تحقق:', citationsDropped: 'أُسقط مصدر',
  },
  documents: { loading: '…', error: 'خطأ' },
};

describe('ChatPage', () => {
  let http: HttpTestingController;
  let authed: boolean;

  beforeEach(async () => {
    authed = true;
    await TestBed.configureTestingModule({
      imports: [
        ChatPage,
        TranslocoTestingModule.forRoot({
          langs: { ar: translations },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [
        provideZonelessChangeDetection(), provideRouter([]), provideHttpClient(), provideHttpClientTesting(),
        // Only the one signal the page reads - the real service would drag in OAuth wiring.
        { provide: AuthService, useValue: { isAuthenticated: () => authed } },
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  function send(fixture: ReturnType<typeof TestBed.createComponent<ChatPage>>, question: string) {
    const el = fixture.nativeElement as HTMLElement;
    const input = el.querySelector<HTMLInputElement>('input[name="question"]')!;
    input.value = question;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    el.querySelector<HTMLButtonElement>('[data-testid="send"]')!.click();
    fixture.detectChanges();
    return http.expectOne('/api/guides/chat/ask');
  }

  it('signed out, it offers sign-in instead of a broken chat', async () => {
    authed = false;
    const fixture = TestBed.createComponent(ChatPage);
    fixture.detectChanges();
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="signin-prompt"]')).toBeTruthy();
    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="send"]')).toBeNull();
  });

  it('a grounded answer renders with citation links into the guide and freshness', async () => {
    const fixture = TestBed.createComponent(ChatPage);
    fixture.detectChanges();

    const post = send(fixture, 'كم الرسوم؟');
    expect(post.request.body).toEqual({ question: 'كم الرسوم؟' });
    post.flush({
      answered: true,
      answer: 'الرسوم 300 ريال.',
      citations: [{
        chunkId: 'c1', guideVersionId: 'v1', guideSlug: 'renew-passport',
        titleAr: 'تجديد جواز السفر', titleEn: 'Renew a passport',
        snippet: '300 ريال', lastVerifiedAt: '2026-09-01',
      }],
      lastVerifiedAt: '2026-09-01',
      hallucinatedCitationsDropped: true,
    });
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('الرسوم 300 ريال.');
    const citationLink = el.querySelector('a[href*="renew-passport"]');
    expect(citationLink?.textContent).toContain('تجديد جواز السفر');    // the door into the guide
    expect(el.textContent).toContain('آخر تحقق:');                       // freshness on the citation
    expect(el.querySelector('[data-testid="dropped-warning"]')).toBeTruthy();
  });

  it('a refusal renders the honest message, not an empty answer', async () => {
    const fixture = TestBed.createComponent(ChatPage);
    fixture.detectChanges();

    send(fixture, 'ما عاصمة فرنسا؟').flush({
      answered: false, answer: null, message: 'لم أجد إجابة موثوقة.',
      citations: [], lastVerifiedAt: null, hallucinatedCitationsDropped: false,
    });
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('[data-testid="refusal"]')?.textContent)
      .toContain('لم أجد إجابة موثوقة.');
  });
});
