# Checkpoint log

| Date | Phase | Question | Verdict | Note |
| --- | --- | --- | --- | --- |
| 2026-08-27 | 0 | Six modules + the decoupling rule (and how it is enforced) | Not attempted — answer requested | Retry from memory next session; review learning docs 0.3–0.5 |
| 2026-08-27 | 0 | Six modules + the decoupling rule (and enforcement) | Pass | Q1: named mechanism (own tables/DbContext) instead of the rule itself ("no cross-module DB joins") — corrected. Q2 (follow-up, reminder idempotency): named the Status=Pending gate but missed the `UQ_Reminder_DocumentId_OffsetDays` unique index that prevents duplicate rows — corrected. |
