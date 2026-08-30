/// Chip severity as a pure function of the server-computed daysUntilExpiry (the server owns
/// the calendar math - DB6/UTC rules live in ONE place; the UI only classifies). Pure and
/// exported so tests hit it directly, the ExtractedValueParser lesson client-side.
export type ExpirySeverity = 'expired' | 'soon' | 'ok' | 'none';

export function expirySeverity(daysUntilExpiry: number | null | undefined): ExpirySeverity {
  if (daysUntilExpiry == null) {
    return 'none';
  }
  if (daysUntilExpiry < 0) {
    return 'expired';
  }
  return daysUntilExpiry <= 30 ? 'soon' : 'ok';
}

/// Tailwind classes per severity - logical/color utilities only, safe under both dir values.
export const EXPIRY_CHIP_CLASSES: Record<ExpirySeverity, string> = {
  expired: 'bg-red-100 text-red-800',
  soon: 'bg-amber-100 text-amber-800',
  ok: 'bg-emerald-100 text-emerald-800',
  none: 'bg-slate-100 text-slate-600',
};
