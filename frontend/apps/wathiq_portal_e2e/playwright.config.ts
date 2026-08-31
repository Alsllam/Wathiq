import { defineConfig } from '@playwright/test';

/**
 * PRECONDITION (documented, not automated - the WATHIQ_OLLAMA_SMOKE philosophy): the backend
 * stack must be running (SQL + host on https://localhost:44352 with the seeded admin user).
 * The portal itself IS auto-served below. On a machine with pre-provisioned browsers, point
 * PW_EXECUTABLE_PATH at the chromium binary; otherwise `npx playwright install chromium` once.
 */
export default defineConfig({
  testDir: './src',
  outputDir: '../../dist/.playwright/wathiq_portal_e2e',
  timeout: 120_000,
  retries: 0,
  use: {
    baseURL: 'http://localhost:4200',
    ignoreHTTPSErrors: true, // the dev backend's self-signed cert, crossed during login redirects
    launchOptions: {
      executablePath: process.env['PW_EXECUTABLE_PATH'] || undefined,
      args: ['--ignore-certificate-errors'],
    },
  },
  webServer: {
    command: 'npx nx serve wathiq_portal --port 4200',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
    timeout: 180_000,
  },
});
