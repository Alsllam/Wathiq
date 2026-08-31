import { Component, computed, effect, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient, httpResource } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import {
  DocumentDto,
  ListResultDto,
  PagedResultDto,
  ReminderDto,
  ReminderRuleDto,
  injectApiUrl,
} from '@wathiq/shared/api';
import { LanguageService } from '@wathiq/shared/i18n';
import { RuleEditorState } from './rule-editor-state';
import { TimelineGroup, buildTimeline } from './timeline';

@Component({
  selector: 'wq-reminders-page',
  imports: [TranslocoPipe, DatePipe, RouterLink],
  template: `
    <section class="grid gap-8 lg:grid-cols-[1fr_20rem]">
      <!-- ————— The timeline: three computeds deep, zero stored intermediates ————— -->
      <div>
        <h2 class="text-xl font-semibold text-slate-900">{{ 'reminders.title' | transloco }}</h2>

        @if (upcoming.isLoading()) {
          <p class="mt-4 text-sm text-slate-500">{{ 'documents.loading' | transloco }}</p>
        } @else if (upcoming.error()) {
          <p class="mt-4 text-sm text-amber-700">{{ 'documents.error' | transloco }}</p>
        } @else {
          @for (group of timeline(); track group.monthKey) {
            <h3 class="mt-6 text-sm font-semibold text-slate-500">
              {{ group.monthKey + '-01' | date: 'MMMM y' : undefined : language.locale() }}
            </h3>
            <ul class="mt-2 space-y-1">
              @for (row of group.rows; track row.reminder.id) {
                <li>
                  <a [routerLink]="['/documents', row.reminder.documentId]"
                     class="flex items-center gap-3 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm hover:bg-slate-50">
                    <!-- Overdue is a flag on the row, not a separate list: one timeline, honest. -->
                    <span class="size-2 rounded-full"
                          [class.bg-red-500]="row.overdue" [class.bg-emerald-500]="!row.overdue"></span>
                    <span class="min-w-0 flex-1 truncate text-start">{{ documentLabel(row.reminder) }}</span>
                    <span class="text-xs text-slate-500" dir="ltr">
                      {{ row.reminder.dueDate | date: 'mediumDate' : undefined : language.locale() }}
                    </span>
                    <span class="rounded-full px-2 py-0.5 text-xs font-medium"
                          [class.bg-red-100]="row.overdue" [class.text-red-800]="row.overdue"
                          [class.bg-slate-100]="!row.overdue" [class.text-slate-600]="!row.overdue">
                      {{ (row.overdue ? 'reminders.overdueBy' : 'reminders.inDays') | transloco: { days: abs(row.daysAway) } }}
                    </span>
                  </a>
                </li>
              }
            </ul>
          } @empty {
            <p class="mt-4 text-sm text-slate-500">{{ 'reminders.empty' | transloco }}</p>
          }
        }
      </div>

      <!-- ————— The rule editor: 2.2's value objects with edit affordances ————— -->
      <aside class="rounded-2xl border border-slate-200 bg-white p-5">
        <h3 class="text-sm font-semibold text-slate-900">{{ 'reminders.settings' | transloco }}</h3>

        <p class="mt-4 text-xs font-medium text-slate-500">{{ 'reminders.offsets' | transloco }}</p>
        <div class="mt-2 flex flex-wrap items-center gap-1.5">
          @for (offset of state.offsets(); track offset) {
            <span class="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-xs text-emerald-800">
              {{ 'reminders.days' | transloco: { days: offset } }}
              <button type="button" class="text-emerald-600 hover:text-emerald-900"
                      [attr.aria-label]="'reminders.remove' | transloco" (click)="state.removeOffset(offset)">×</button>
            </span>
          }
          <input type="number" min="1" max="365" class="w-16 rounded border border-slate-300 px-1.5 py-0.5 text-xs"
                 dir="ltr" data-testid="new-offset"
                 [value]="state.newOffset()" (input)="state.newOffset.set(asValue($event))"
                 (keydown.enter)="state.addOffset()" />
          <button type="button" class="rounded border border-slate-300 px-1.5 py-0.5 text-xs"
                  (click)="state.addOffset()">+</button>
        </div>

        <p class="mt-4 text-xs font-medium text-slate-500">{{ 'reminders.channels' | transloco }}</p>
        <label class="mt-1 flex items-center gap-2 text-sm">
          <input type="checkbox" [checked]="state.email()" (change)="state.email.set(asChecked($event))" />
          {{ 'reminders.email' | transloco }}
        </label>
        <label class="mt-1 flex items-center gap-2 text-sm text-slate-400">
          <input type="checkbox" disabled />
          {{ 'reminders.push' | transloco }} <span class="text-xs">({{ 'reminders.soon' | transloco }})</span>
        </label>

        <p class="mt-4 text-xs font-medium text-slate-500">{{ 'reminders.quiet' | transloco }}</p>
        <div class="mt-1 flex items-center gap-2" dir="ltr">
          <input type="time" class="rounded border border-slate-300 px-2 py-1 text-sm"
                 [value]="state.quietFrom()" (input)="state.quietFrom.set(asValue($event))" />
          <span class="text-slate-400">–</span>
          <input type="time" class="rounded border border-slate-300 px-2 py-1 text-sm"
                 [value]="state.quietTo()" (input)="state.quietTo.set(asValue($event))" />
        </div>
        @if (state.quietPairBroken()) {
          <p class="mt-1 text-xs text-red-700">{{ 'reminders.quietPair' | transloco }}</p>
        }

        <p class="mt-4 text-xs font-medium text-slate-500">{{ 'reminders.timezone' | transloco }}</p>
        <select class="mt-1 w-full rounded border border-slate-300 px-2 py-1 text-sm" dir="ltr"
                [value]="state.timeZoneId()" (change)="state.timeZoneId.set(asValue($event))">
          @for (tz of timeZones; track tz) {
            <option [value]="tz" [selected]="tz === state.timeZoneId()">{{ tz }}</option>
          }
        </select>

        <button type="button" data-testid="save-rule"
                class="mt-5 w-full rounded-lg bg-emerald-600 px-3 py-2 text-sm font-medium text-white disabled:opacity-40"
                [disabled]="!state.valid() || saving()" (click)="save()">
          {{ (saved() ? 'reminders.saved' : 'reminders.save') | transloco }}
        </button>
      </aside>
    </section>
  `,
})
export class RemindersPage {
  protected readonly language = inject(LanguageService);
  private readonly http = inject(HttpClient);
  private readonly apiUrl = injectApiUrl();

  /// The browser knows every IANA zone - no bundled list to drift (and the server validates).
  protected readonly timeZones = Intl.supportedValuesOf('timeZone');

  protected readonly state = new RuleEditorState();
  protected readonly saving = signal(false);
  protected readonly saved = signal(false);

  readonly upcoming = httpResource<ListResultDto<ReminderDto>>(() =>
    this.apiUrl('/api/reminders/reminders/upcoming-list')
  );
  private readonly rule = httpResource<ReminderRuleDto>(() => this.apiUrl('/api/reminders/rule'));
  // Join source for row labels: reminders carry documentId only (no cross-module data on the
  // wire, ADR-001) - the page composes the two reads.
  private readonly documents = httpResource<PagedResultDto<DocumentDto>>(() =>
    this.apiUrl('/api/documents/documents?MaxResultCount=100')
  );

  readonly timeline = computed<TimelineGroup[]>(() =>
    buildTimeline(this.upcoming.value()?.items ?? [], new Date().toISOString().slice(0, 10))
  );

  private readonly documentNumbers = computed(() => {
    const map = new Map<string, DocumentDto>();
    for (const d of this.documents.value()?.items ?? []) {
      map.set(d.id!, d);
    }
    return map;
  });

  private seeded = false;

  constructor() {
    // A LEGITIMATE effect (the 4.2 taxonomy): copying arrived server data into edit state that
    // must then DIVERGE from its source. computed can't diverge; an unguarded effect would
    // re-clobber the user's edits on any refetch - hence seed-once.
    effect(() => {
      const rule = this.rule.value();
      if (rule && !this.seeded) {
        this.seeded = true;
        this.state.loadFrom(rule);
      }
    });
  }

  protected documentLabel(reminder: ReminderDto): string {
    const doc = this.documentNumbers().get(reminder.documentId!);
    return doc?.number ?? doc?.notes ?? '…';
  }

  protected abs(n: number): number {
    return Math.abs(n);
  }

  protected asValue(event: Event): string {
    return (event.target as HTMLInputElement).value;
  }

  protected asChecked(event: Event): boolean {
    return (event.target as HTMLInputElement).checked;
  }

  protected save(): void {
    this.saving.set(true);
    this.saved.set(false);
    this.http.put(this.apiUrl('/api/reminders/rule'), this.state.toDto()).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
        this.upcoming.reload(); // the rule change resynced reminders server-side (2.3) - show it
      },
      error: () => this.saving.set(false),
    });
  }
}
