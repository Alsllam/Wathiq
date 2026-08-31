import { expect, test, Page } from '@playwright/test';
import * as path from 'node:path';

/** The 4.4 lesson as a helper: the dev login page AUTOFILLS demo credentials after load -
 * fill only after it settles, and assert what will actually post. */
async function signIn(page: Page): Promise<void> {
  await page.goto('/');
  await page.getByText('تسجيل الدخول').click();
  await page.waitForURL('**/Account/Login**');
  await page.waitForTimeout(1500);
  await page.fill('#LoginInput_UserNameOrEmailAddress', 'admin');
  await page.fill('#LoginInput_Password', '1q2w3E*');
  expect(await page.inputValue('#LoginInput_UserNameOrEmailAddress')).toBe('admin');
  await page.click('button[type="submit"][name="Action"]');
  await page.waitForURL('http://localhost:4200/**');
}

test('UC-01 happy path: sign in, create with upload, see it everywhere', async ({ page }) => {
  await signIn(page);
  await expect(page.locator('header')).toContainText('admin');

  // Unique per run - the assertion at the end must find THIS run's document.
  const number = `E2E-${Date.now()}`;

  await page.getByText('الوثائق', { exact: true }).click();
  await page.getByTestId('add-document').click();

  // Scoped to the wizard: type names also exist in the shell's catalogue preview.
  const wizard = page.locator('wq-add-document-wizard');
  await wizard.getByText('جواز السفر').click();
  await wizard.locator('button.rounded-full').first().click(); // the self holder
  await wizard.getByText('التالي').click();

  await wizard.locator('input[dir="ltr"]').fill(number);
  const dates = wizard.locator('input[type="date"]');
  await dates.nth(0).fill('2026-01-01');
  await dates.nth(1).fill('2036-01-01');
  await wizard.getByText('التالي').click();

  await page.setInputFiles('#wizard-file', path.join(__dirname, 'fixtures/scan.png'));
  await page.getByTestId('finish').click();

  // Lands on the fresh detail: number, chip, attachment row.
  await page.waitForURL(/documents\/(?!new)[0-9a-f-]+/);
  const detail = page.locator('wq-document-detail');
  await expect(detail).toContainText(number);
  await expect(detail).toContainText('image/png');

  // Back on the list, this run's document is a row.
  await page.getByText('عودة إلى الوثائق').click();
  await expect(page.locator('wq-documents-list')).toContainText(number);

  // The reminders timeline renders (rows exist because the new expiry resynced reminders).
  await page.getByText('التذكيرات', { exact: true }).click();
  await expect(page.locator('wq-reminders-page')).toContainText('الجدول الزمني');
});
