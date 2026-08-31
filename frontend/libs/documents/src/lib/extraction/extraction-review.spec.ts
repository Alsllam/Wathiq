import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { ExtractionReview } from './extraction-review';

const translations = {
  extraction: {
    start: 'استخراج', notReady: 'لم تكتمل القراءة', retryIn: 'خلال {{s}}', working: 'جارٍ',
    failed: 'تعذّر', capped: 'بلغت الحد', retry: 'إعادة', reviewTitle: 'راجِع',
    confidence: 'ثقة {{p}}', confirm: 'تأكيد', reject: 'رفض', done: 'تم', unsupported: 'غير متاح',
  },
  wizard: { number: 'الرقم' },
  documents: { issueDate: 'الإصدار', expiryDate: 'الانتهاء' },
};

describe('ExtractionReview', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [
        ExtractionReview,
        TranslocoTestingModule.forRoot({
          langs: { ar: translations },
          translocoConfig: { availableLangs: ['ar', 'en'], defaultLang: 'ar' },
        }),
      ],
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  function create() {
    const fixture = TestBed.createComponent(ExtractionReview);
    fixture.componentRef.setInput('documentId', 'doc-1');
    fixture.componentRef.setInput('attachmentId', 'att-1');
    fixture.detectChanges();
    return fixture;
  }

  const extractUrl = '/api/documents/document-extraction/doc-1/extract/att-1';

  it('renders the proposal with its warnings and pre-fills the editable fields', async () => {
    const fixture = create();
    fixture.componentInstance.extract();
    http.expectOne(extractUrl).flush({
      extractionResultId: 'x1',
      number: 'P-99',
      issueDate: null,
      expiryDate: '2036-03-01',
      confidence: 0.8,
      warnings: ["Issue date '30/02/2026' is not a valid YYYY-MM-DD date - dropped."],
    });
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    // FR-AI-003 as UX: the dropped field arrives EMPTY and the reason is on screen.
    expect(el.querySelector('[data-testid="warnings"]')?.textContent).toContain('30/02/2026');
    expect(fixture.componentInstance.number()).toBe('P-99');
    expect(fixture.componentInstance.issueDate()).toBe('');
    expect(fixture.componentInstance.expiryDate()).toBe('2036-03-01');
  });

  it('ExtractionNotReady auto-retries on a visible countdown', async () => {
    jest.useFakeTimers();
    try {
      const fixture = create();
      fixture.componentInstance.extract();
      http.expectOne(extractUrl).flush(
        { error: { code: 'Wathiq.Documents:ExtractionNotReady' } },
        { status: 403, statusText: 'Forbidden' }
      );
      // No whenStable() under fake timers - the zoneless scheduler itself uses timers, so
      // stability would wait forever. HTTP-testing flushes deliver synchronously; signals are
      // already set.
      expect(fixture.componentInstance.phase()).toBe('waitingOcr');

      jest.advanceTimersByTime(8000); // the countdown elapses -> a second attempt fires
      http.expectOne(extractUrl).flush({ extractionResultId: 'x2', warnings: [] });
      expect(fixture.componentInstance.phase()).toBe('review');
    } finally {
      jest.useRealTimers();
    }
  });

  it('maps the cap code to its message and offers no retry', async () => {
    const fixture = create();
    fixture.componentInstance.extract();
    http.expectOne(extractUrl).flush(
      { error: { code: 'Wathiq.Ai:DailyCapExceeded' } },
      { status: 403, statusText: 'Forbidden' }
    );
    await fixture.whenStable();

    expect(fixture.componentInstance.errorKey()).toBe('extraction.capped');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('بلغت الحد');
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('إعادة المحاولة');
  });

  it('confirm posts the edited values and emits concluded', async () => {
    const fixture = create();
    const concluded = jest.fn();
    fixture.componentInstance.concluded.subscribe(concluded);

    fixture.componentInstance.extract();
    http.expectOne(extractUrl).flush({ extractionResultId: 'x1', number: 'P-99', warnings: [] });
    await fixture.whenStable();

    fixture.componentInstance.expiryDate.set('2035-01-01'); // the user corrected the model
    fixture.componentInstance.confirm();
    const post = http.expectOne('/api/documents/document-extraction/doc-1/confirm/x1');
    expect(post.request.body).toEqual({ number: 'P-99', issueDate: null, expiryDate: '2035-01-01' });
    post.flush({});
    await fixture.whenStable();

    expect(concluded).toHaveBeenCalled();
    expect(fixture.componentInstance.phase()).toBe('concluded');
  });
});
