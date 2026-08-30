import { AddDocumentWizardState, fileIssue } from './wizard-state';

describe('AddDocumentWizardState', () => {
  it('gates each step on its own validity', () => {
    const s = new AddDocumentWizardState();

    s.next();
    expect(s.step()).toBe(1); // nothing chosen - no move

    s.typeId.set('t1');
    s.holderId.set('h1');
    s.next();
    expect(s.step()).toBe(2);

    s.issueDate.set('2030-01-01');
    s.expiryDate.set('2020-01-01');
    expect(s.datesInverted()).toBe(true); // the ExpiryBeforeIssue rule, client-side mirror
    s.next();
    expect(s.step()).toBe(2); // blocked

    s.expiryDate.set('2036-01-01');
    s.next();
    expect(s.step()).toBe(3);

    s.back();
    expect(s.step()).toBe(2);
  });
});

describe('fileIssue (the server allow-list, mirrored)', () => {
  it.each([
    [null, null],
    [{ type: 'image/png', size: 100 }, null],
    [{ type: 'IMAGE/JPEG', size: 100 }, null],       // case-insensitive like the server's gate
    [{ type: 'text/plain', size: 100 }, 'type'],
    [{ type: 'application/pdf', size: 21 * 1024 * 1024 }, 'size'],
  ] as const)('%p -> %p', (file, expected) => {
    expect(fileIssue(file as never)).toBe(expected);
  });
});
