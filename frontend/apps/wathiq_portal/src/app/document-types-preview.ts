import { Component, inject } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { TranslocoPipe } from '@jsverse/transloco';
import { DocumentTypeDto, ListResultDto, injectApiUrl } from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';

/// 4.3's smoke and the httpResource first contact: what used to be a service method + a
/// subscription + three fields (data/loading/error) is ONE declarative primitive - the request
/// is described, and value()/isLoading()/error() are signals zoneless CD tracks natively.
@Component({
  selector: 'app-document-types-preview',
  imports: [TranslocoPipe],
  template: `
    <section class="rounded-2xl border border-slate-200 bg-white p-6">
      <h2 class="text-base font-semibold text-slate-900">
        {{ 'catalogue.title' | transloco }}
      </h2>

      @if (types.isLoading()) {
        <p class="mt-3 text-sm text-slate-500">{{ 'catalogue.loading' | transloco }}</p>
      } @else if (types.error()) {
        <!-- The backend being down is a normal dev state - say so instead of a blank box. -->
        <p class="mt-3 text-sm text-amber-700">{{ 'catalogue.error' | transloco }}</p>
      } @else {
        <ul class="mt-4 grid gap-2 sm:grid-cols-2">
          @for (type of types.value()?.items; track type.id) {
            <li class="flex items-center gap-2 rounded-lg bg-slate-50 px-3 py-2 text-sm">
              <span class="size-1.5 rounded-full bg-emerald-500"></span>
              <!-- One catalogue, two names: the active language picks the column, not the API. -->
              <span>{{ language.lang() === 'ar' ? type.nameAr : type.nameEn }}</span>
            </li>
          } @empty {
            <li class="text-sm text-slate-500">{{ 'catalogue.empty' | transloco }}</li>
          }
        </ul>
      }
    </section>
  `,
})
export class DocumentTypesPreview {
  protected readonly language = inject(LanguageService);
  private readonly apiUrl = injectApiUrl();

  // Typed by the GENERATED contract: if the backend renames nameAr, `npm run generate:api`
  // turns this component into a compile error instead of an undefined at runtime.
  readonly types = httpResource<ListResultDto<DocumentTypeDto>>(
    () => this.apiUrl('/api/documents/document-types')
  );
}
