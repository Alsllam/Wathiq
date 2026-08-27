---
title: "Wathiq — Docs Pipeline Smoke Test"
subtitle: "وثيق — اختبار خط إنتاج المستندات"
author: "Abdulsalam"
version: "0.1"
date: "2026-08-27"
status: "Draft"
---

# Purpose

This document exists only to prove that Markdown renders to a Word file with the project
template: headings, tables, code, and right-to-left Arabic paragraphs.

## Revision history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 0.1 | 2026-08-27 | Abdulsalam | First render |

## Bilingual paragraphs

English body text uses the Latin font; the Arabic block below is a fenced div with the `RTL`
custom style, which maps to a Word paragraph style with `bidi` set.

::: {custom-style="RTL"}
وثيق خدمة مجانية تساعدك على متابعة مستنداتك الشخصية (الهوية، الجواز، الرخصة، التأمين)
وتذكّرك قبل انتهاء صلاحيتها، وتجيب على سؤال «كيف أجدد؟» بالعربية والإنجليزية.
:::

## Code and lists

```csharp
public sealed record ExpiryDate(DateOnly Value);
```

1. Markdown source lives in `docs/deliverables/<key>.md`.
2. `build.ps1 <key>` renders it to `out/<Key>.docx`.
3. Both are committed, so Word files travel with the repo.
