import { Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient, httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { GuideDetailDto, injectApiUrl } from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';
import { parseGuideBody } from './guide-body';

@Component({
  selector: 'wq-guide-detail',
  imports: [DatePipe, RouterLink, TranslocoPipe],
  template: `
    <a routerLink=".." class="text-sm text-emerald-700 hover:underline">{{ 'guides.back' | transloco }}</a>

    @if (guide.isLoading()) {
      <p class="mt-6 text-slate-500">{{ 'documents.loading' | transloco }}</p>
    } @else if (guide.error()) {
      <p class="mt-6 text-red-600">{{ 'documents.error' | transloco }}</p>
    } @else if (guide.value(); as g) {
      <!-- The generated type marks version optional; one guard narrows it for the whole article. -->
      @if (g.version; as v) {
      <article class="mt-4">
        <h2 class="text-2xl font-semibold text-slate-900">
          {{ lang.lang() === 'ar' ? g.titleAr : g.titleEn }}
        </h2>
        <!-- Vision R2 in the UI: freshness is the first thing under the title, not a footnote. -->
        <p class="mt-1 text-sm text-emerald-700" data-testid="freshness">
          {{ 'guides.lastVerified' | transloco }} {{ v.lastVerifiedAt | date: 'mediumDate' : undefined : lang.locale() }}
        </p>

        @if (v.requiredDocuments || v.fees || v.location) {
          <dl class="mt-4 grid gap-2 rounded-xl bg-slate-50 p-4 text-sm sm:grid-cols-3">
            @if (v.requiredDocuments; as v) {
              <div><dt class="font-medium text-slate-500">{{ 'guides.requiredDocuments' | transloco }}</dt><dd class="mt-1 text-slate-900">{{ v }}</dd></div>
            }
            @if (v.fees; as v) {
              <div><dt class="font-medium text-slate-500">{{ 'guides.fees' | transloco }}</dt><dd class="mt-1 text-slate-900">{{ v }}</dd></div>
            }
            @if (v.location; as v) {
              <div><dt class="font-medium text-slate-500">{{ 'guides.location' | transloco }}</dt><dd class="mt-1 text-slate-900">{{ v }}</dd></div>
            }
          </dl>
        }

        <div class="mt-4 space-y-2">
          @for (segment of body(); track $index) {
            @switch (segment.kind) {
              @case ('heading') { <h3 class="pt-2 text-lg font-semibold text-slate-900">{{ segment.text }}</h3> }
              @case ('bullet') { <p class="ms-4 text-slate-700">• {{ segment.text }}</p> }
              @default { <p class="text-slate-700">{{ segment.text }}</p> }
            }
          }
        </div>

        @if (v.steps?.length) {
          <h3 class="mt-6 text-lg font-semibold text-slate-900">{{ 'guides.steps' | transloco }}</h3>
          <ol class="mt-2 space-y-2">
            @for (step of v.steps; track $index) {
              <li class="flex gap-3">
                <span class="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-emerald-100 text-sm font-medium text-emerald-800">{{ $index + 1 }}</span>
                <span class="text-slate-700">{{ step }}</span>
              </li>
            }
          </ol>
        }

        <!-- The 5.6 feedback loop's reader end: anonymous by design, one honest button. -->
        <div class="mt-8 border-t border-slate-200 pt-4">
          @if (flagged()) {
            <p class="text-sm text-emerald-700" data-testid="flag-thanks">{{ 'guides.flagThanks' | transloco }}</p>
          } @else {
            <button type="button" data-testid="flag-outdated" (click)="flagOutdated(g)"
                    class="rounded-lg border border-amber-300 px-3 py-1.5 text-sm text-amber-800 hover:bg-amber-50">
              {{ 'guides.flagOutdated' | transloco }}
            </button>
          }
        </div>
      </article>
      }
    }
  `,
})
export class GuideDetail {
  protected readonly lang = inject(LanguageService);
  private readonly http = inject(HttpClient);
  private readonly apiUrl = injectApiUrl();

  readonly slug = input.required<string>();   // withComponentInputBinding: the :slug route param

  // The URL derives from TWO signals: switching language refetches the right version - the
  // backend serves latest-published per language, the UI just declares the dependency.
  readonly guide = httpResource<GuideDetailDto>(
    () => this.apiUrl(`/api/guides/guide/by-slug?slug=${encodeURIComponent(this.slug())}&language=${this.lang.lang()}`)
  );

  readonly body = computed(() => parseGuideBody(this.guide.value()?.version?.bodyMarkdown ?? ''));

  protected readonly flagged = signal(false);

  flagOutdated(g: GuideDetailDto): void {
    // Kind 0 = Outdated. Fire-and-thank: the admin queue is the other end (5.6).
    this.http.post(this.apiUrl('/api/guides/guide-feedback'), { guideVersionId: g.version!.id, kind: 0 })
      .subscribe({ next: () => this.flagged.set(true) });
  }
}
