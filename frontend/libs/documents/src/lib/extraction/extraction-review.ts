import { Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { TranslocoPipe } from '@jsverse/transloco';
import { ExtractionProposalDto, injectApiUrl } from '@wathiq/shared/api';

/// ABP's error contract: the machine-readable code travels in error.code - map codes to i18n
/// keys, NEVER match on message strings (they are localized).
const ERROR_KEYS: Record<string, string> = {
  'Wathiq.Documents:ExtractionNotReady': 'extraction.notReady',
  'Wathiq.Documents:ExtractionFailed': 'extraction.failed',
  'Wathiq.Ai:DailyCapExceeded': 'extraction.capped',
};

const RETRY_SECONDS = 8;
const MAX_AUTO_RETRIES = 10;

type Phase = 'idle' | 'waitingOcr' | 'extracting' | 'review' | 'failed' | 'concluded';

@Component({
  selector: 'wq-extraction-review',
  imports: [TranslocoPipe],
  template: `
    <div class="mt-2 rounded-xl border border-slate-200 bg-slate-50 p-4">
      @switch (phase()) {
        @case ('idle') {
          <button type="button" data-testid="extract"
                  class="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-700"
                  (click)="extract()">
            {{ 'extraction.start' | transloco }}
          </button>
        }
        @case ('waitingOcr') {
          <!-- OCR readiness only surfaces through the extract attempt (ExtractionNotReady), so
               the "poll" is a visible auto-retry - the state says so instead of spinning. -->
          <p class="text-sm text-slate-600">
            {{ 'extraction.notReady' | transloco }} · {{ 'extraction.retryIn' | transloco: { s: countdown() } }}
          </p>
        }
        @case ('extracting') {
          <p class="text-sm text-slate-600">{{ 'extraction.working' | transloco }}</p>
        }
        @case ('failed') {
          <p class="text-sm text-amber-700">{{ errorKey() | transloco }}</p>
          @if (errorKey() !== 'extraction.capped') {
            <button type="button" class="mt-2 rounded-lg border border-slate-300 px-3 py-1.5 text-sm"
                    (click)="extract()">
              {{ 'extraction.retry' | transloco }}
            </button>
          }
        }
        @case ('review') {
          <p class="text-sm font-medium text-slate-900">{{ 'extraction.reviewTitle' | transloco }}</p>
          @if (proposal()?.confidence; as confidence) {
            <p class="text-xs text-slate-500">{{ 'extraction.confidence' | transloco: { p: (confidence * 100).toFixed(0) } }}</p>
          }

          <!-- FR-AI-003 as UX: every dropped value explains itself, right above the empty field. -->
          @if (proposal()?.warnings?.length) {
            <ul class="mt-2 space-y-1" data-testid="warnings">
              @for (warning of proposal()?.warnings; track warning) {
                <li class="text-xs text-amber-700" dir="auto">⚠ {{ warning }}</li>
              }
            </ul>
          }

          <div class="mt-3 grid gap-3">
            <label class="block text-sm">
              <span class="text-slate-700">{{ 'wizard.number' | transloco }}</span>
              <input class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-1.5" dir="ltr"
                     [value]="number()" (input)="number.set(asValue($event))" />
            </label>
            <div class="grid gap-3 sm:grid-cols-2">
              <label class="block text-sm">
                <span class="text-slate-700">{{ 'documents.issueDate' | transloco }}</span>
                <input type="date" class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-1.5"
                       [value]="issueDate()" (input)="issueDate.set(asValue($event))" />
              </label>
              <label class="block text-sm">
                <span class="text-slate-700">{{ 'documents.expiryDate' | transloco }}</span>
                <input type="date" class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-1.5"
                       [value]="expiryDate()" (input)="expiryDate.set(asValue($event))" />
              </label>
            </div>
          </div>

          <div class="mt-3 flex items-center gap-2">
            <button type="button" data-testid="confirm"
                    class="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-700"
                    [disabled]="busy()" (click)="confirm()">
              {{ 'extraction.confirm' | transloco }}
            </button>
            <button type="button" data-testid="reject"
                    class="rounded-lg border border-slate-300 px-3 py-1.5 text-sm text-slate-700"
                    [disabled]="busy()" (click)="reject()">
              {{ 'extraction.reject' | transloco }}
            </button>
          </div>
        }
        @case ('concluded') {
          <p class="text-sm text-emerald-700">{{ 'extraction.done' | transloco }}</p>
        }
      }
    </div>
  `,
})
export class ExtractionReview {
  readonly documentId = input.required<string>();
  readonly attachmentId = input.required<string>();
  /// Fires after confirm/reject so the parent can document.reload() - the review never touches
  /// the parent's data directly.
  readonly concluded = output<void>();

  private readonly http = inject(HttpClient);
  private readonly apiUrl = injectApiUrl();
  private readonly destroyRef = inject(DestroyRef);

  readonly phase = signal<Phase>('idle');
  readonly errorKey = signal('extraction.failed');
  readonly countdown = signal(RETRY_SECONDS);
  readonly proposal = signal<ExtractionProposalDto | null>(null);
  readonly busy = signal(false);

  // The editable copy, pre-filled from the proposal on arrival (4.6's field-signal pattern).
  readonly number = signal('');
  readonly issueDate = signal('');
  readonly expiryDate = signal('');

  private retries = 0;
  private timer: ReturnType<typeof setInterval> | null = null;

  constructor() {
    this.destroyRef.onDestroy(() => this.stopTimer());
  }

  extract(): void {
    this.stopTimer();
    this.phase.set('extracting');
    this.http
      .post<ExtractionProposalDto>(
        this.apiUrl(`/api/documents/document-extraction/${this.documentId()}/extract/${this.attachmentId()}`),
        null
      )
      .subscribe({
        next: (proposal) => {
          this.proposal.set(proposal);
          this.number.set(proposal.number ?? '');
          this.issueDate.set(proposal.issueDate ?? '');
          this.expiryDate.set(proposal.expiryDate ?? '');
          this.phase.set('review');
        },
        error: (err: HttpErrorResponse) => {
          const code = err.error?.error?.code as string | undefined;
          if (code === 'Wathiq.Documents:ExtractionNotReady' && this.retries < MAX_AUTO_RETRIES) {
            this.scheduleRetry(); // OCR still running - poll by retrying, visibly
          } else {
            this.errorKey.set(ERROR_KEYS[code ?? ''] ?? 'extraction.failed');
            this.phase.set('failed');
          }
        },
      });
  }

  confirm(): void {
    this.busy.set(true);
    this.http
      .post(
        this.apiUrl(`/api/documents/document-extraction/${this.documentId()}/confirm/${this.proposal()!.extractionResultId}`),
        {
          number: this.number() || null,
          issueDate: this.issueDate() || null,
          expiryDate: this.expiryDate() || null,
        }
      )
      .subscribe({
        next: () => {
          this.phase.set('concluded');
          this.concluded.emit(); // server recorded Accepted or Edited; reminders resynced (2.4)
        },
        error: () => {
          this.busy.set(false);
          this.errorKey.set('extraction.failed');
          this.phase.set('failed');
        },
      });
  }

  reject(): void {
    this.busy.set(true);
    this.http
      .post(this.apiUrl(`/api/documents/document-extraction/${this.documentId()}/reject/${this.proposal()!.extractionResultId}`), null)
      .subscribe({
        next: () => {
          this.phase.set('concluded');
          this.concluded.emit();
        },
        error: () => this.busy.set(false),
      });
  }

  protected asValue(event: Event): string {
    return (event.target as HTMLInputElement).value;
  }

  /// setInterval's only job is writing signals - zoneless CD tracks the reads, no zone patching.
  private scheduleRetry(): void {
    this.retries++;
    this.phase.set('waitingOcr');
    this.countdown.set(RETRY_SECONDS);
    this.timer = setInterval(() => {
      this.countdown.set(this.countdown() - 1);
      if (this.countdown() <= 0) {
        this.extract();
      }
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timer !== null) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }
}
