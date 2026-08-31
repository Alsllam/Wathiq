import { computed, signal } from '@angular/core';
import { ReminderRuleDto, UpdateReminderRuleDto } from '@wathiq/shared/api';

export const EMAIL_FLAG = 1;
export const PUSH_FLAG = 2;

/// The 2.2 value objects, editing-side: offsets as one array signal (the ReminderOffsets CSV,
/// now chips), channels decomposed into booleans and recomposed into flags, quiet hours as a
/// both-or-neither pair (IsQuietAt's contract). Validity mirrors the domain rules at the
/// friendlier moment; the server remains the authority.
export class RuleEditorState {
  readonly offsets = signal<number[]>([]);
  readonly email = signal(true);
  readonly quietFrom = signal(''); // "HH:mm" from <input type=time>, '' = off
  readonly quietTo = signal('');
  readonly timeZoneId = signal('Asia/Riyadh');

  readonly newOffset = signal('');

  readonly quietPairBroken = computed(
    () => (this.quietFrom() === '') !== (this.quietTo() === '')
  );
  readonly valid = computed(
    () => this.offsets().length > 0 && !this.quietPairBroken() && this.timeZoneId() !== ''
  );

  loadFrom(rule: ReminderRuleDto): void {
    this.offsets.set([...(rule.offsetsDays ?? [])].sort((a, b) => b - a));
    this.email.set(((rule.channels ?? 0) & EMAIL_FLAG) !== 0);
    this.quietFrom.set(rule.quietFrom?.slice(0, 5) ?? ''); // server sends HH:mm:ss
    this.quietTo.set(rule.quietTo?.slice(0, 5) ?? '');
    this.timeZoneId.set(rule.timeZoneId ?? 'Asia/Riyadh');
  }

  addOffset(): void {
    const value = Number(this.newOffset());
    if (Number.isInteger(value) && value > 0 && value <= 365 && !this.offsets().includes(value)) {
      this.offsets.set([...this.offsets(), value].sort((a, b) => b - a));
    }
    this.newOffset.set('');
  }

  removeOffset(value: number): void {
    this.offsets.set(this.offsets().filter((o) => o !== value));
  }

  toDto(): UpdateReminderRuleDto {
    return {
      offsetsDays: this.offsets(),
      channels: (this.email() ? EMAIL_FLAG : 0), // Push joins in P6
      quietFrom: this.quietFrom() ? `${this.quietFrom()}:00` : null,
      quietTo: this.quietTo() ? `${this.quietTo()}:00` : null,
      timeZoneId: this.timeZoneId(),
    };
  }
}
