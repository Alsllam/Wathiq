/// The portal's expirySeverity (4.5), ported: pure input→enum, no clock read -
/// the API computes daysUntilExpiry server-side, so both apps agree by
/// construction instead of by duplicated date math.
enum ExpirySeverity { none, ok, soon, expired }

ExpirySeverity expirySeverity(int? daysUntilExpiry) {
  return switch (daysUntilExpiry) {
    null => ExpirySeverity.none, // documents without an expiry are fine, not scary
    < 0 => ExpirySeverity.expired,
    <= 30 => ExpirySeverity.soon,
    _ => ExpirySeverity.ok,
  };
}
