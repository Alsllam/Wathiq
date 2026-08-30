import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpEventType, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { AddDocumentWizard } from './add-document-wizard';

const translations = {
  wizard: {
    title: 'إضافة وثيقة', stepOf: 'الخطوة {{step}}', type: 'النوع', holder: 'الصاحب', number: 'الرقم',
    datesInverted: 'مقلوبة', attachment: 'مرفق', back: 'السابق', next: 'التالي',
    createOnly: 'إنشاء', createWithFile: 'إنشاء ورفع', error: 'خطأ',
    file: { type: 'نوع غير مدعوم', size: 'كبير' },
  },
  documents: { issueDate: 'الإصدار', expiryDate: 'الانتهاء', notes: 'ملاحظات' },
};

describe('AddDocumentWizard', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [
        AddDocumentWizard,
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

  it('walks create + upload with progress and lands on the detail route', async () => {
    const fixture = TestBed.createComponent(AddDocumentWizard);
    const navigate = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.detectChanges(); // resources fire (4.3 ordering)
    http.expectOne('/api/documents/document-types').flush({
      items: [{ id: 't1', code: 'PASSPORT', nameAr: 'جواز', nameEn: 'Passport' }],
    });
    http.expectOne('/api/documents/holders').flush({
      items: [{ id: 'h1', fullName: 'أمينة', isSelf: true }],
    });
    await fixture.whenStable();

    const el = fixture.nativeElement as HTMLElement;
    const component = fixture.componentInstance as never as {
      state: { typeId: { set(v: string): void }; holderId: { set(v: string): void }; next(): void; file: { set(v: File): void } };
      uploadProgress: () => number | null;
    };

    // Drive the state like the buttons do (the state machine itself is spec'd separately).
    component.state.typeId.set('t1');
    component.state.holderId.set('h1');
    component.state.next();
    component.state.next();
    component.state.file.set(new File([new Uint8Array(4)], 'scan.png', { type: 'image/png' }));
    await fixture.whenStable();

    el.querySelector<HTMLButtonElement>('[data-testid="finish"]')!.click();
    await fixture.whenStable();

    http.expectOne('/api/documents/documents').flush({ id: 'doc-1' });
    await fixture.whenStable();

    const upload = http.expectOne('/api/documents/documents/doc-1/upload-attachment');
    upload.event({ type: HttpEventType.UploadProgress, loaded: 50, total: 100 });
    await fixture.whenStable();
    expect(component.uploadProgress()).toBe(50); // the RxJS event became a signal

    upload.event({ type: HttpEventType.Response } as never);
    upload.flush({ id: 'att-1' });
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith(['/documents', 'doc-1']);
  });
});
