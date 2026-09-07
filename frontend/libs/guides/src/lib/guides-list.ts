import { Component, inject } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { GuideDto, ListResultDto, injectApiUrl } from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';

@Component({
  selector: 'wq-guides-list',
  imports: [RouterLink, TranslocoPipe],
  template: `
    <section>
      <div class="flex items-center">
        <h2 class="text-xl font-semibold text-slate-900">{{ 'guides.title' | transloco }}</h2>
        <a routerLink="chat" data-testid="open-chat"
           class="ms-auto rounded-lg bg-emerald-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-700">
          {{ 'guides.askButton' | transloco }}
        </a>
      </div>
      <p class="mt-1 text-sm text-slate-500">{{ 'guides.subtitle' | transloco }}</p>

      @if (guides.isLoading()) {
        <p class="mt-6 text-slate-500">{{ 'documents.loading' | transloco }}</p>
      } @else if (guides.error()) {
        <p class="mt-6 text-red-600">{{ 'documents.error' | transloco }}</p>
      } @else {
        <ul class="mt-4 grid gap-3 sm:grid-cols-2">
          @for (guide of guides.value()?.items; track guide.id) {
            <li>
              <a [routerLink]="[guide.slug]"
                 class="block rounded-xl border border-slate-200 bg-white p-4 hover:border-emerald-400">
                <span class="font-medium text-slate-900">
                  {{ lang.lang() === 'ar' ? guide.titleAr : guide.titleEn }}
                </span>
                <span class="mt-1 block text-sm text-slate-500">
                  {{ lang.lang() === 'ar' ? guide.titleEn : guide.titleAr }}
                </span>
              </a>
            </li>
          } @empty {
            <li class="text-slate-500">{{ 'guides.empty' | transloco }}</li>
          }
        </ul>
      }
    </section>
  `,
})
export class GuidesList {
  protected readonly lang = inject(LanguageService);
  private readonly apiUrl = injectApiUrl();

  // Public read - the resource needs no auth and the page renders signed-out.
  readonly guides = httpResource<ListResultDto<GuideDto>>(() => this.apiUrl('/api/guides/guide'));
}
