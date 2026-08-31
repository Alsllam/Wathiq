import { RuleEditorState } from './rule-editor-state';

describe('RuleEditorState', () => {
  it('round-trips a rule through load and toDto', () => {
    const state = new RuleEditorState();
    state.loadFrom({
      offsetsDays: [7, 90, 30],
      channels: 1,
      quietFrom: '22:00:00',
      quietTo: '07:00:00',
      timeZoneId: 'Asia/Riyadh',
    });

    expect(state.offsets()).toEqual([90, 30, 7]); // display order: largest offset first
    expect(state.quietFrom()).toBe('22:00');      // HH:mm:ss trimmed for <input type=time>

    expect(state.toDto()).toEqual({
      offsetsDays: [90, 30, 7],
      channels: 1,
      quietFrom: '22:00:00',                       // and re-suffixed on the way out
      quietTo: '07:00:00',
      timeZoneId: 'Asia/Riyadh',
    });
  });

  it('offset chips: add validates and dedupes, remove removes', () => {
    const state = new RuleEditorState();
    state.loadFrom({ offsetsDays: [30], timeZoneId: 'Asia/Riyadh' });

    state.newOffset.set('7');
    state.addOffset();
    state.newOffset.set('7');
    state.addOffset(); // duplicate - ignored
    state.newOffset.set('400');
    state.addOffset(); // out of range - ignored
    expect(state.offsets()).toEqual([30, 7]);

    state.removeOffset(30);
    expect(state.offsets()).toEqual([7]);
  });

  it('mirrors the domain validity rules', () => {
    const state = new RuleEditorState();
    state.loadFrom({ offsetsDays: [30], timeZoneId: 'Asia/Riyadh' });
    expect(state.valid()).toBe(true);

    state.quietFrom.set('22:00'); // half a pair - the IsQuietAt contract needs both
    expect(state.quietPairBroken()).toBe(true);
    expect(state.valid()).toBe(false);
    state.quietTo.set('07:00');
    expect(state.valid()).toBe(true);

    state.removeOffset(30); // no offsets = no reminders = not a rule
    expect(state.valid()).toBe(false);
  });
});
