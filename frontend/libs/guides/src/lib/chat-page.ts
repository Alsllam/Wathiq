import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { GuideChatResponseDto, injectApiUrl } from '@wathiq/shared/api';
import { AuthService } from '@wathiq/shared/auth';
import { LanguageService } from '@wathiq/shared/i18n';

/// One Q&A exchange. The response is kept whole: citations, freshness and the warning flag are
/// the product, not decoration - rendering them IS UC-03's trust story.
interface ChatExchange {
  question: string;
  response?: GuideChatResponseDto;
  failed?: boolean;
}

@Component({
  selector: 'wq-chat-page',
  imports: [DatePipe, RouterLink, TranslocoPipe],
  template: `
    <a routerLink=".." class="text-sm text-emerald-700 hover:underline">{{ 'guides.back' | transloco }}</a>
    <h2 class="mt-2 text-xl font-semibold text-slate-900">{{ 'guides.chatTitle' | transloco }}</h2>
    <p class="mt-1 text-sm text-slate-500">{{ 'guides.chatSubtitle' | transloco }}</p>

    @if (!auth.isAuthenticated()) {
      <!-- Chat rides the per-user AI cap (5.5), so it needs an identity; reading never does. -->
      <p class="mt-6 rounded-xl bg-slate-50 p-4 text-slate-600" data-testid="signin-prompt">
        {{ 'guides.chatSignIn' | transloco }}
      </p>
    } @else {
      <div class="mt-4 space-y-4">
        @for (exchange of exchanges(); track $index) {
          <div class="rounded-xl bg-emerald-50 p-3 text-slate-900">{{ exchange.question }}</div>

          @if (exchange.failed) {
            <p class="text-sm text-red-600">{{ 'documents.error' | transloco }}</p>
          } @else if (!exchange.response) {
            <p class="text-sm text-slate-500">{{ 'guides.thinking' | transloco }}</p>
          } @else if (!exchange.response.answered) {
            <div class="rounded-xl border border-slate-200 p-3 text-slate-700" data-testid="refusal">
              {{ exchange.response.message }}
            </div>
          } @else {
            <div class="rounded-xl border border-emerald-200 bg-white p-3">
              <p class="whitespace-pre-line text-slate-900">{{ exchange.response.answer }}</p>

              @if (exchange.response.hallucinatedCitationsDropped) {
                <p class="mt-2 text-xs text-amber-700" data-testid="dropped-warning">
                  {{ 'guides.citationsDropped' | transloco }}
                </p>
              }

              <!-- Citations link INTO the guide: every claim has a door the reader can open. -->
              <div class="mt-3 border-t border-slate-100 pt-2">
                <p class="text-xs font-medium text-slate-500">{{ 'guides.sources' | transloco }}</p>
                <ul class="mt-1 space-y-1">
                  @for (citation of exchange.response.citations; track citation.chunkId) {
                    <li class="text-sm">
                      <a [routerLink]="['..', citation.guideSlug]" class="text-emerald-700 hover:underline">
                        {{ lang.lang() === 'ar' ? citation.titleAr : citation.titleEn }}
                      </a>
                      <span class="text-xs text-slate-500">
                        · {{ 'guides.lastVerified' | transloco }}
                        {{ citation.lastVerifiedAt | date: 'mediumDate' : undefined : lang.locale() }}
                      </span>
                    </li>
                  }
                </ul>
              </div>
            </div>
          }
        }
      </div>

      <form class="mt-6 flex gap-2" (submit)="ask($event)">
        <input name="question" [value]="draft()" (input)="draft.set($any($event.target).value)"
               [placeholder]="'guides.askPlaceholder' | transloco" maxlength="512"
               class="min-w-0 flex-1 rounded-lg border border-slate-300 px-3 py-2 focus:border-emerald-500 focus:outline-none" />
        <button type="submit" data-testid="send" [disabled]="pending() || !draft().trim()"
                class="rounded-lg bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-50">
          {{ 'guides.send' | transloco }}
        </button>
      </form>
    }
  `,
})
export class ChatPage {
  protected readonly auth = inject(AuthService);
  protected readonly lang = inject(LanguageService);
  private readonly http = inject(HttpClient);
  private readonly apiUrl = injectApiUrl();

  // Imperative POST, not httpResource: asking is an ACTION with history, not a re-derivable
  // view of server state (the wizard's lesson, applied to conversation).
  protected readonly exchanges = signal<ChatExchange[]>([]);
  protected readonly draft = signal('');
  protected readonly pending = signal(false);

  ask(event: Event): void {
    event.preventDefault();
    const question = this.draft().trim();
    if (!question || this.pending()) {
      return;
    }

    this.draft.set('');
    this.pending.set(true);
    this.exchanges.update((list) => [...list, { question }]);

    this.http.post<GuideChatResponseDto>(this.apiUrl('/api/guides/chat/ask'), { question }).subscribe({
      next: (response) => {
        this.exchanges.update((list) =>
          list.map((e, i) => (i === list.length - 1 ? { ...e, response } : e)));
        this.pending.set(false);
      },
      error: () => {
        this.exchanges.update((list) =>
          list.map((e, i) => (i === list.length - 1 ? { ...e, failed: true } : e)));
        this.pending.set(false);
      },
    });
  }
}
