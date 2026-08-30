import { computed, signal } from '@angular/core';

/// Mirror of the server's allow-list (DocumentConsts.AllowedMimeTypes + FileStore MaxSizeBytes).
/// The server remains the authority - this only saves the user a 20 MB round trip to learn no.
export const ALLOWED_MIME_TYPES = ['image/jpeg', 'image/png', 'application/pdf'];
export const MAX_SIZE_BYTES = 20 * 1024 * 1024;

export type FileIssue = 'type' | 'size' | null;

export function fileIssue(file: { type: string; size: number } | null): FileIssue {
  if (!file) {
    return null;
  }
  if (!ALLOWED_MIME_TYPES.includes(file.type.toLowerCase())) {
    return 'type';
  }
  return file.size > MAX_SIZE_BYTES ? 'size' : null;
}

/// The wizard's brain, framework-light on purpose: fields are signals, validity is computed,
/// and the component only reads/writes - so THIS class is unit-testable without a DOM. It is
/// what "signal forms" means before the experimental @angular/forms/signals API exists (v21+):
/// the concept, hand-rolled.
export class AddDocumentWizardState {
  readonly step = signal<1 | 2 | 3>(1);

  // Step 1
  readonly typeId = signal<string | null>(null);
  readonly holderId = signal<string | null>(null);
  readonly step1Valid = computed(() => this.typeId() !== null && this.holderId() !== null);

  // Step 2
  readonly number = signal('');
  readonly issueDate = signal('');   // yyyy-MM-dd from <input type=date>, '' = not set
  readonly expiryDate = signal('');
  readonly notes = signal('');
  /// The ExpiryBeforeIssue rule, client-side: same rule as ValidityPeriod, caught at the
  /// friendlier moment. The server still enforces it - this is UX, not trust.
  readonly datesInverted = computed(
    () => this.issueDate() !== '' && this.expiryDate() !== '' && this.expiryDate() < this.issueDate()
  );
  readonly step2Valid = computed(() => !this.datesInverted());

  // Step 3
  readonly file = signal<File | null>(null);
  readonly fileIssue = computed(() => fileIssue(this.file()));
  readonly fileOk = computed(() => this.file() !== null && this.fileIssue() === null);

  back(): void {
    if (this.step() > 1) {
      this.step.set((this.step() - 1) as 1 | 2);
    }
  }

  next(): void {
    if (this.step() === 1 && this.step1Valid()) {
      this.step.set(2);
    } else if (this.step() === 2 && this.step2Valid()) {
      this.step.set(3);
    }
  }
}
