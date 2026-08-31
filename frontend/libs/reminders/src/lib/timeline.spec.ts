import { buildTimeline } from './timeline';

describe('buildTimeline', () => {
  const reminders = [
    { id: 'r3', documentId: 'd1', dueDate: '2026-10-05' },
    { id: 'r1', documentId: 'd1', dueDate: '2026-08-20' }, // overdue vs today below
    { id: 'r2', documentId: 'd2', dueDate: '2026-09-01' },
    { id: 'r4', documentId: 'd2', dueDate: '2026-10-20' },
  ];

  it('sorts soonest-first, flags overdue, groups by month', () => {
    const groups = buildTimeline(reminders, '2026-09-01');

    expect(groups.map((g) => g.monthKey)).toEqual(['2026-08', '2026-09', '2026-10']);
    expect(groups[0].rows[0].overdue).toBe(true);
    expect(groups[0].rows[0].daysAway).toBe(-12);
    expect(groups[1].rows[0].daysAway).toBe(0); // due today = not overdue yet
    expect(groups[1].rows[0].overdue).toBe(false);
    expect(groups[2].rows.map((r) => r.reminder.id)).toEqual(['r3', 'r4']);
  });

  it('is empty-safe', () => {
    expect(buildTimeline([], '2026-09-01')).toEqual([]);
  });
});
