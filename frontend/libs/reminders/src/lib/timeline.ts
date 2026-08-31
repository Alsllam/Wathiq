import { ReminderDto } from '@wathiq/shared/api';

/// Pure timeline derivation (the expiry.ts pattern at lib scale): sort soonest-first, flag
/// overdue against a caller-supplied "today" (tests own the clock), group into month buckets.
export interface TimelineRow {
  reminder: ReminderDto;
  daysAway: number;   // negative = overdue
  overdue: boolean;
}

export interface TimelineGroup {
  monthKey: string;   // "2036-03" - stable for @for track; the UI formats per locale
  rows: TimelineRow[];
}

export function buildTimeline(reminders: readonly ReminderDto[], todayIso: string): TimelineGroup[] {
  const today = new Date(todayIso + 'T00:00:00Z').getTime();
  const rows = [...reminders]
    .filter((r) => r.dueDate)
    .map((reminder) => {
      const daysAway = Math.round((new Date(reminder.dueDate + 'T00:00:00Z').getTime() - today) / 86_400_000);
      return { reminder, daysAway, overdue: daysAway < 0 };
    })
    .sort((a, b) => a.reminder.dueDate!.localeCompare(b.reminder.dueDate!));

  const groups: TimelineGroup[] = [];
  for (const row of rows) {
    const monthKey = row.reminder.dueDate!.slice(0, 7);
    const last = groups[groups.length - 1];
    if (last?.monthKey === monthKey) {
      last.rows.push(row);
    } else {
      groups.push({ monthKey, rows: [row] });
    }
  }
  return groups;
}
