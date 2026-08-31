import { Component, inject, input } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpClient, httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AttachmentDto, DocumentDto, injectApiUrl } from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';
import { ExtractionReview } from './extraction/extraction-review';
import { EXPIRY_CHIP_CLASSES, expirySeverity } from './expiry';

@Component({
  selector: 'wq-document-detail',
  imports: [RouterLink, TranslocoPipe, DatePipe, DecimalPipe, ExtractionReview],
  template: `
    <section>
      <a routerLink=".." class="text-sm text-emerald-700 hover:underline">
        ‹ {{ 'documents.back' | transloco }}
      </a>

      @if (document.isLoading()) {
        <p class="mt-4 text-sm text-slate-500">{{ 'documents.loading' | transloco }}</p>
      } @else if (document.error()) {
        <p class="mt-4 text-sm text-amber-700">{{ 'documents.error' | transloco }}</p>
      } @else if (document.value(); as doc) {
        <div class="mt-4 rounded-2xl border border-slate-200 bg-white p-6">
          <div class="flex items-center gap-3">
            <h2 class="text-lg font-semibold text-slate-900" dir="ltr">{{ doc.number ?? '—' }}</h2>
            <span class="rounded-full px-2.5 py-0.5 text-xs font-medium {{ chipClass(doc) }}">
              {{ chipKey(doc) | transloco: { days: absDays(doc) } }}
            </span>
          </div>

          <dl class="mt-4 grid gap-x-8 gap-y-2 text-sm sm:grid-cols-2">
            <div>
              <dt class="text-slate-500">{{ 'documents.issueDate' | transloco }}</dt>
              <!-- The 4.2 trick: locale as a PIPE ARGUMENT read from a signal - the pure pipe
                   re-runs on language switch because its input changed. -->
              <dd>{{ doc.issueDate ? (doc.issueDate | date: 'mediumDate' : undefined : language.locale()) : '—' }}</dd>
            </div>
            <div>
              <dt class="text-slate-500">{{ 'documents.expiryDate' | transloco }}</dt>
              <dd>{{ doc.expiryDate ? (doc.expiryDate | date: 'mediumDate' : undefined : language.locale()) : '—' }}</dd>
            </div>
            @if (doc.notes) {
              <div class="sm:col-span-2">
                <dt class="text-slate-500">{{ 'documents.notes' | transloco }}</dt>
                <dd>{{ doc.notes }}</dd>
              </div>
            }
          </dl>

          <h3 class="mt-6 text-sm font-semibold text-slate-900">
            {{ 'documents.attachments' | transloco }}
          </h3>
          <ul class="mt-2 space-y-1">
            @for (att of doc.attachments; track att.id) {
              <li>
                <div class="flex items-center gap-3 rounded-lg bg-slate-50 px-3 py-2 text-sm">
                  <span class="text-slate-700" dir="ltr">{{ att.mimeType }}</span>
                  <span class="text-slate-400">{{ (att.sizeBytes ?? 0) / 1024 | number: '1.0-0' }} KB</span>
                  <button type="button" class="ms-auto text-emerald-700 hover:underline"
                          (click)="download(doc.id!, att)">
                    {{ 'documents.download' | transloco }}
                  </button>
                </div>
                @if (att.mimeType?.startsWith('image/')) {
                  <!-- UC-01's last mile: review the AI's proposal, then confirm through the same
                       endpoints 3.7 proved - concluded() reloads the document (resource.reload). -->
                  <wq-extraction-review [documentId]="doc.id!" [attachmentId]="att.id!"
                                        (concluded)="document.reload()" />
                } @else {
                  <p class="mt-1 text-xs text-slate-400">{{ 'extraction.unsupported' | transloco }}</p>
                }
              </li>
            } @empty {
              <li class="text-sm text-slate-500">{{ 'documents.noAttachments' | transloco }}</li>
            }
          </ul>
        </div>
      }
    </section>
  `,
})
export class DocumentDetail {
  /// The route param, delivered as a SIGNAL INPUT by withComponentInputBinding - no
  /// ActivatedRoute subscription; navigating between ids just changes the input.
  readonly id = input.required<string>();

  protected readonly language = inject(LanguageService);
  private readonly http = inject(HttpClient);
  private readonly apiUrl = injectApiUrl();

  readonly document = httpResource<DocumentDto>(() =>
    this.apiUrl(`/api/documents/documents/${this.id()}`)
  );

  /// Blob download must ride HttpClient (the interceptor attaches the token; a plain <a href>
  /// would arrive anonymous and 401). Object URL + synthetic click = the browser save dialog.
  protected download(documentId: string, att: AttachmentDto): void {
    this.http
      .get(this.apiUrl(`/api/documents/documents/${documentId}/attachment-content/${att.id}`), {
        responseType: 'blob',
      })
      .subscribe((blob) => {
        const url = URL.createObjectURL(blob);
        const a = Object.assign(document.createElement('a'), { href: url, download: `attachment-${att.id}` });
        a.click();
        URL.revokeObjectURL(url);
      });
  }

  protected chipKey(doc: DocumentDto): string {
    return `documents.expiry.${expirySeverity(doc.daysUntilExpiry)}`;
  }

  protected chipClass(doc: DocumentDto): string {
    return EXPIRY_CHIP_CLASSES[expirySeverity(doc.daysUntilExpiry)];
  }

  protected absDays(doc: DocumentDto): number {
    return Math.abs(doc.daysUntilExpiry ?? 0);
  }
}
