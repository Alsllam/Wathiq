import { setupZonelessTestEnv } from 'jest-preset-angular/setup-env/zoneless';

// Tests run the same change-detection model as the app (app.config.ts): zoneless. With zone.js
// uninstalled, the zone-based setup would not even import.
setupZonelessTestEnv({
  errorOnUnknownElements: true,
  errorOnUnknownProperties: true,
});
