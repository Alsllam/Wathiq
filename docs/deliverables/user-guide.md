---
title: "Wathiq — User Guide"
subtitle: "وثيق — دليل الاستخدام"
author: "Abdulsalam"
version: "0.1"
date: "2026-08-30"
status: "Draft"
---

# Document control {-}

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 0.1 | 2026-08-30 | Abdulsalam | First version covering the web portal's Phase-4 flows (roadmap step 4.9). Screenshots *Planned* once the visual design settles |

**Status:** Draft · This guide describes what the portal **does today**. Features marked
*قريبًا / coming soon* are visible in the app but not yet active.

# 1. What Wathiq is — ما هو وثيق

::: {custom-style="RTL"}
وثيق مساعد مجاني يحفظ وثائقك الشخصية (الهوية، الجواز، الرخصة، الاستمارة، التأمين، العقود…)،
ويذكّرك قبل انتهائها، ويقرأ صورة الوثيقة ليقترح بياناتها بدلًا من كتابتها يدويًا — **دون أن
تغادر صورُ وثائقك الخادم إلى أي خدمة سحابية**.
:::

Wathiq keeps your personal documents in one place, reminds you before anything expires, and
reads a document photo to *propose* its data so you don't type it. Document images never leave
the server to any cloud AI.

# 2. Signing in — تسجيل الدخول

1. Open the portal and press **تسجيل الدخول / Sign in**.
2. You are taken to Wathiq's own secure login page; enter your username and password.
3. You return signed in — your name appears in the header, next to **الوثائق / Documents** and
   **التذكيرات / Reminders**.

The **English / العربية** button switches language instantly; the whole layout mirrors for
Arabic (right-to-left) automatically.

# 3. Adding a document — إضافة وثيقة

From **الوثائق**, press **إضافة وثيقة / Add document**. Three short steps:

1. **Type & holder** — pick what the document is (passport, licence…) and whose it is.
2. **Details** — number and dates, all optional. If you attach a photo next, the assistant can
   fill these for you afterwards. The form warns immediately if the expiry precedes the issue date.
3. **Attachment** — a JPEG/PNG photo or a PDF, up to 20 MB. Unsupported files are refused
   *before* uploading. A progress bar tracks the upload.

You land on the document's page. Reminders for its expiry are scheduled automatically.

# 4. Letting the AI read it — الاستخراج الذكي

On a document's page, each photo attachment has **استخراج البيانات بالمساعد الذكي / Extract
data with AI**.

- If the photo is still being read (text recognition), the button waits and retries by itself —
  you see a countdown, not a frozen screen.
- The assistant then shows a **proposal**: number and dates it read, its confidence, and — when
  it could not read something safely — an empty field *with the reason* (for example, an
  impossible date it refused to guess).
- **Nothing is saved until you confirm.** Edit anything, then **تأكيد وحفظ / Confirm & save** —
  or **رفض الاقتراح / Reject**. On confirm, the document updates and its reminders reschedule.
- If you reach the daily AI limit, the button says so and stops — your documents are unaffected.

# 5. The expiry timeline and reminder settings — التذكيرات

**التذكيرات / Reminders** shows every upcoming reminder, soonest first, grouped by month —
overdue items are flagged in red. Each row links to its document.

In **إعدادات التذكير / Reminder settings** you control:

- **Offsets** — how many days before expiry to remind you (e.g. 90, 30, 7, 1). Add or remove
  freely; the timeline updates the moment you save.
- **Channels** — email today; mobile push is *قريبًا / coming soon*.
- **Quiet hours** — reminders due in this window wait until it ends (set both times, or neither).
- **Time zone** — reminder days are computed in *your* time zone.

# 6. Glossary — المسرد

| العربية | English |
| --- | --- |
| وثيقة | Document |
| مرفق | Attachment |
| تاريخ الانتهاء | Expiry date |
| التذكير | Reminder |
| ساعات الهدوء | Quiet hours |
| الاستخراج | Extraction (AI reading) |
| اقتراح | Proposal (what the AI suggests) |

# 7. Coming soon — قريبًا {-}

Mobile app with camera capture and push notifications (Phase 6) · renewal guides with a
grounded Q&A assistant (Phase 5) · sharing a document securely (Phase 8).
