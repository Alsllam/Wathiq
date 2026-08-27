---
name: learn
description: Give a focused, code-anchored explanation of a topic from the Wathiq learning plan (ABP, EF Core, Hangfire, Microsoft.Extensions.AI, RAG, Angular signals, Flutter/Riverpod, SQL Server, Docker, privacy). Use when the user invokes /learn <topic> or asks "explain X" while building.
---

# learn

Teach one topic, anchored to this repo, without writing product code.

## Procedure

1. **Scope the topic** to one idea (if the request is broad, pick the piece relevant to the
   current or next roadmap step and say so).
2. **Anchor it**: find where the topic already appears (or will appear) in this repo — cite
   files and lines. If nothing exists yet, use the reference repos
   (`d:\Projects\MoD.HousingProject.Frontend`, `D:\Projects\MoD.PoC.HousingProject`) for the
   "what you already know" side.
3. **Explain in this order**, briefly:
   - *What you already know that this maps to* (Angular / ABP / SQL / .NET background).
   - *The core idea* in ≤ 10 lines.
   - *A minimal example* in this project's terms (10–25 lines of code, runnable or nearly).
   - *The trap* — the one mistake people make with it, and how this repo's guardrails avoid it.
   - *Check yourself* — one question, answer hidden behind `<details>`.
4. **Offer, don't do**: end with the roadmap step where this will be applied. Do not modify
   product code or commit. If the user wants the explanation kept, save it as
   `docs/learning/topics/<slug>.md` and commit `learn(<slug>): <topic>`.

## Tone

Peer explaining at a whiteboard. No fluff, no history lessons, concrete over abstract.
