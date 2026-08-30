import { Component, inject, signal } from '@angular/core';
import { HttpClient, HttpEventType, httpResource } from '@angular/common/http';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import {
  DocumentDto,
  DocumentTypeDto,
  HolderDto,
  ListResultDto,
  injectApiUrl,
} from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';
import { AddDocumentWizardState } from './wizard-state';

@Component({
  selector: 'wq-add-document-wizard',
  imports: [TranslocoPipe],
  template: `
    <section class="mx-auto max-w-xl">
      <h2 class="text-xl font-semibold text-slate-900">{{ 'wizard.title' | transloco }}</h2>
      <!-- The step indicator derives from the same signal that gates the panels. -->
      <p class="mt-1 text-sm text-slate-500">{{ 'wizard.stepOf' | transloco: { step: state.step() } }}</p>

      <div class="mt-4 rounded-2xl border border-slate-200 bg-white p-6">
        @switch (state.step()) {
          @case (1) {
            <p class="block text-sm font-medium text-slate-700">{{ 'wizard.type' | transloco }}</p>
            <div class="mt-2 grid gap-2 sm:grid-cols-2">
              @for (type of types.value()?.items; track type.id) {
                <button type="button"
                        class="rounded-lg border px-3 py-2 text-start text-sm"
                        [class.border-emerald-600]="state.typeId() === type.id"
                        [class.bg-emerald-50]="state.typeId() === type.id"
                        [class.border-slate-300]="state.typeId() !== type.id"
                        (click)="state.typeId.set(type.id!)">
                  {{ language.lang() === 'ar' ? type.nameAr : type.nameEn }}
                </button>
              }
            </div>

            <p class="mt-4 block text-sm font-medium text-slate-700">{{ 'wizard.holder' | transloco }}</p>
            <div class="mt-2 flex flex-wrap gap-2">
              @for (holder of holders.value()?.items; track holder.id) {
                <button type="button"
                        class="rounded-full border px-3 py-1.5 text-sm"
                        [class.border-emerald-600]="state.holderId() === holder.id"
                        [class.bg-emerald-50]="state.holderId() === holder.id"
                        [class.border-slate-300]="state.holderId() !== holder.id"
                        (click)="state.holderId.set(holder.id!)">
                  {{ holder.fullName }}
                </button>
              }
            </div>
          }
          @case (2) {
            <div class="grid gap-4">
              <label class="block text-sm">
                <span class="font-medium text-slate-700">{{ 'wizard.number' | transloco }}</span>
                <!-- value + input listener IS the signal-forms binding: no directive layer. -->
                <input class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2" dir="ltr"
                       [value]="state.number()" (input)="state.number.set(asValue($event))" />
              </label>
              <div class="grid gap-4 sm:grid-cols-2">
                <label class="block text-sm">
                  <span class="font-medium text-slate-700">{{ 'documents.issueDate' | transloco }}</span>
                  <input type="date" class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
                         [value]="state.issueDate()" (input)="state.issueDate.set(asValue($event))" />
                </label>
                <label class="block text-sm">
                  <span class="font-medium text-slate-700">{{ 'documents.expiryDate' | transloco }}</span>
                  <input type="date" class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2"
                         [value]="state.expiryDate()" (input)="state.expiryDate.set(asValue($event))" />
                </label>
              </div>
              @if (state.datesInverted()) {
                <p class="text-sm text-red-700">{{ 'wizard.datesInverted' | transloco }}</p>
              }
              <label class="block text-sm">
                <span class="font-medium text-slate-700">{{ 'documents.notes' | transloco }}</span>
                <textarea class="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2" rows="2"
                          [value]="state.notes()" (input)="state.notes.set(asValue($event))"></textarea>
              </label>
            </div>
          }
          @case (3) {
            <label for="wizard-file" class="block text-sm font-medium text-slate-700">{{ 'wizard.attachment' | transloco }}</label>
            <input id="wizard-file" type="file" class="mt-2 block w-full text-sm" accept="image/jpeg,image/png,application/pdf"
                   (change)="onFile($event)" />
            @if (state.fileIssue(); as issue) {
              <!-- Same rejection the server would send (UnsupportedFileType / size cap), caught
                   before a single byte uploads. -->
              <p class="mt-2 text-sm text-red-700">{{ 'wizard.file.' + issue | transloco }}</p>
            }
            @if (uploadProgress() !== null) {
              <div class="mt-4 h-2 overflow-hidden rounded-full bg-slate-100" dir="ltr">
                <div class="h-full bg-emerald-600 transition-all" [style.width.%]="uploadProgress()"></div>
              </div>
              <p class="mt-1 text-xs text-slate-500">{{ uploadProgress() }}%</p>
            }
            @if (error()) {
              <p class="mt-2 text-sm text-red-700">{{ 'wizard.error' | transloco }}</p>
            }
          }
        }
      </div>

      <div class="mt-4 flex items-center gap-3">
        @if (state.step() > 1) {
          <button type="button" class="rounded-lg border border-slate-300 px-4 py-2 text-sm"
                  [disabled]="busy()" (click)="state.back()">
            {{ 'wizard.back' | transloco }}
          </button>
        }
        <!-- ms-auto: the primary action sits at the reading-direction end on both languages. -->
        @if (state.step() < 3) {
          <button type="button" class="ms-auto rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-40"
                  [disabled]="(state.step() === 1 && !state.step1Valid()) || (state.step() === 2 && !state.step2Valid())"
                  (click)="state.next()">
            {{ 'wizard.next' | transloco }}
          </button>
        } @else {
          <button type="button" class="ms-auto rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-40"
                  [disabled]="busy() || (state.file() !== null && !state.fileOk())"
                  data-testid="finish" (click)="finish()">
            {{ state.file() ? ('wizard.createWithFile' | transloco) : ('wizard.createOnly' | transloco) }}
          </button>
        }
      </div>
    </section>
  `,
})
export class AddDocumentWizard {
  protected readonly language = inject(LanguageService);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = injectApiUrl();

  protected readonly state = new AddDocumentWizardState();
  protected readonly busy = signal(false);
  protected readonly error = signal(false);
  /// null = no upload happening; 0-100 while the browser streams the file up.
  protected readonly uploadProgress = signal<number | null>(null);

  protected readonly types = httpResource<ListResultDto<DocumentTypeDto>>(() =>
    this.apiUrl('/api/documents/document-types')
  );
  protected readonly holders = httpResource<ListResultDto<HolderDto>>(() =>
    this.apiUrl('/api/documents/holders')
  );

  protected asValue(event: Event): string {
    return (event.target as HTMLInputElement).value;
  }

  protected onFile(event: Event): void {
    this.state.file.set((event.target as HTMLInputElement).files?.[0] ?? null);
  }

  /// Create, then (optionally) upload with progress, then land on the new detail page.
  protected finish(): void {
    this.busy.set(true);
    this.error.set(false);

    this.http
      .post<DocumentDto>(this.apiUrl('/api/documents/documents'), {
        documentTypeId: this.state.typeId(),
        holderId: this.state.holderId(),
        number: this.state.number() || null,
        issueDate: this.state.issueDate() || null,
        expiryDate: this.state.expiryDate() || null,
        notes: this.state.notes() || null,
      })
      .subscribe({
        next: (doc) => {
          const file = this.state.file();
          if (!file) {
            this.router.navigate(['/documents', doc.id]);
            return;
          }
          const form = new FormData();
          form.append('file', file, file.name);
          // The one RxJS stream left on this screen: the browser's upload progress events,
          // bridged INTO a signal at the edge - the template only ever sees uploadProgress().
          this.http
            .post(this.apiUrl(`/api/documents/documents/${doc.id}/upload-attachment`), form, {
              reportProgress: true,
              observe: 'events',
            })
            .subscribe({
              next: (event) => {
                if (event.type === HttpEventType.UploadProgress && event.total) {
                  this.uploadProgress.set(Math.round((100 * event.loaded) / event.total));
                }
              },
              complete: () => this.router.navigate(['/documents', doc.id]),
              error: () => {
                // The DOCUMENT exists; only the file failed - land on detail, let the user retry
                // from there rather than resubmitting the whole wizard (and duplicating the doc).
                this.router.navigate(['/documents', doc.id]);
              },
            });
        },
        error: () => {
          this.busy.set(false);
          this.error.set(true);
        },
      });
  }
}
