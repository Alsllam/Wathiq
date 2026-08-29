import { setupZonelessTestEnv } from 'jest-preset-angular/setup-env/zoneless';

// Same CD model as the app (4.1): zone.js is not even installed in this workspace.
setupZonelessTestEnv({
  errorOnUnknownElements: true,
  errorOnUnknownProperties: true,
});
