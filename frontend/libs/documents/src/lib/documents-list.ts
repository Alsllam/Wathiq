import { Component, computed, inject, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import {
  DocumentDto,
  DocumentTypeDto,
  ListResultDto,
  PagedResultDto,
  injectApiUrl,
} from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';
import { EXPIRY_CHIP_CLASSES, expirySeverity } from './expiry';

const PAGE_SIZE = 10;

@Component({
  selector: 'wq-documents-list',
  imports: [RouterLink, TranslocoPipe],
  template: `
    <section>
      <div class="flex items-center">
        <h2 class="text-xl font-semibold text-slate-900">{{ 'documents.title' | transloco }}</h2>
        <a routerLink="new" data-testid="add-document"
           class="ms-auto rounded-lg bg-emerald-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-700">
          {{ 'wizard.title' | transloco }}
        </a>
      </div>

      @if (documents.isLoading()) {
        <p class="mt-4 text-sm text-slate-500">{{ 'documents.loading' | transloco }}</p>
      } @else if (documents.error()) {
        <p class="mt-4 text-sm text-amber-700">{{ 'documents.error' | transloco }}</p>
      } @else {
        <ul class="mt-4 divide-y divide-slate-100 rounded-2xl border border-slate-200 bg-white">
          @for (doc of documents.value()?.items; track doc.id) {
            <li>
              <a [routerLink]="[doc.id]" class="flex items-center gap-4 px-4 py-3 hover:bg-slate-50">
                <div class="min-w-0 flex-1 text-start">
                  <p class="truncate font-medium text-slate-900">{{ typeName(doc) }}</p>
                  <p class="truncate text-sm text-slate-500" dir="ltr">{{ doc.number ?? '—' }}</p>
                </div>
                <!-- The chip: severity + label derive from ONE server number, nothing stored. -->
                <span class="rounded-full px-2.5 py-0.5 text-xs font-medium {{ chipClass(doc) }}">
                  {{ chipKey(doc) | transloco: { days: absDays(doc) } }}
                </span>
              </a>
            </li>
          } @empty {
            <li class="px-4 py-8 text-center text-sm text-slate-500">
              {{ 'documents.empty' | transloco }}
            </li>
          }
        </ul>

        @if (totalPages() > 1) {
          <nav class="mt-4 flex items-center justify-center gap-3 text-sm">
            <!-- Writing the page signal is the ONLY imperative act on this screen: the resource
                 URL reads page(), so a click refetches - no reload method, no subscription. -->
            <button class="rounded border border-slate-300 px-2 py-1 disabled:opacity-40"
                    [disabled]="page() === 0" (click)="page.set(page() - 1)">‹</button>
            <span>{{ page() + 1 }} / {{ totalPages() }}</span>
            <button class="rounded border border-slate-300 px-2 py-1 disabled:opacity-40"
                    [disabled]="page() + 1 >= totalPages()" (click)="page.set(page() + 1)">›</button>
          </nav>
        }
      }
    </section>
  `,
})
export class DocumentsList {
  private readonly apiUrl = injectApiUrl();
  private readonly language = inject(LanguageService);

  readonly page = signal(0);

  // URL is a function of state: page() changes -> new URL -> refetch. That IS the pagination.
  readonly documents = httpResource<PagedResultDto<DocumentDto>>(() =>
    this.apiUrl(`/api/documents/documents?SkipCount=${this.page() * PAGE_SIZE}&MaxResultCount=${PAGE_SIZE}`)
  );

  // Second resource + computed join: the catalogue is tiny and cached by the browser; the map
  // rebuilds only when the catalogue answer changes, not per row.
  private readonly types = httpResource<ListResultDto<DocumentTypeDto>>(() =>
    this.apiUrl('/api/documents/document-types')
  );
  private readonly typeNames = computed(() => {
    const map = new Map<string, DocumentTypeDto>();
    for (const t of this.types.value()?.items ?? []) {
      map.set(t.id!, t);
    }
    return map;
  });

  readonly totalPages = computed(() =>
    Math.ceil((this.documents.value()?.totalCount ?? 0) / PAGE_SIZE)
  );

  protected typeName(doc: DocumentDto): string {
    const type = this.typeNames().get(doc.documentTypeId!);
    return (this.language.lang() === 'ar' ? type?.nameAr : type?.nameEn) ?? '…';
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
