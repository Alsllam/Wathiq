import { expirySeverity } from './expiry';

describe('expirySeverity', () => {
  it.each([
    [null, 'none'],
    [undefined, 'none'],
    [-1, 'expired'],
    [0, 'soon'],   // expires today = act now, not "ok"
    [30, 'soon'],
    [31, 'ok'],
    [365, 'ok'],
  ] as const)('daysUntilExpiry=%p -> %p', (days, expected) => {
    expect(expirySeverity(days)).toBe(expected);
  });
});
