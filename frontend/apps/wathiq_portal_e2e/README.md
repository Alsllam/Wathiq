# wathiq_portal_e2e

The UC-01 happy path against the **real stack** — the browser truths unit tests cannot see
(cookies, antiforgery, OIDC redirects, the dev login autofill).

## Preconditions (not automated — deliberately)

1. SQL Server up with the migrated `Wathiq` database (`docker compose up sql` or LocalDB).
2. Backend host on `https://localhost:44352` (seeded `admin` / `1q2w3E*`).
3. Browsers: `npx playwright install chromium` once — or on a machine with pre-provisioned
   browsers, `PW_EXECUTABLE_PATH=/path/to/chrome`.

The portal dev server is started (or reused) automatically by the Playwright config.

## Run

```sh
npx nx e2e wathiq_portal_e2e
```
