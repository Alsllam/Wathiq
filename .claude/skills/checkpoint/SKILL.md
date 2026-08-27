---
name: checkpoint
description: Run the active phase's checkpoint quiz - the user answers before seeing the answer; on a pass, close the phase in ROADMAP.md and commit. Use at the end of a phase or when the user invokes /checkpoint.
---

# checkpoint

Gate a phase. The point is retrieval practice: the user answers **from memory, before** any
explanation. Never reveal the answer first.

## Procedure

1. **Find the active phase** in `ROADMAP.md`. All its implementation steps must be ticked;
   if not, list what's missing and stop.
2. **Ask the checkpoint question** for that phase from `docs/PLAN.md` §3. Ask conversationally,
   one question at a time, and wait for the answer. Add one or two follow-ups drawn from the
   phase's learning docs ("Check yourself" sections are the source).
3. **Evaluate honestly.**
   - Fully right → say so briefly, add any sharpening detail.
   - Partially right → name exactly what was missing, then give the full answer.
   - Wrong → give the correct answer with a pointer to the learning doc and the code in this
     repo that demonstrates it. Recommend (don't force) re-reading before continuing.
4. **Record the result.** On a pass (fully or mostly right):
   - Tick the `N.CP` box, change the phase label from `active` to `done`, mark the next phase
     `active` in ROADMAP.md.
   - Append an entry to `docs/learning/checkpoints.md` (create if missing): date, phase,
     question, verdict, one-line note on any gap.
   - Confirm the phase's deliverable docs (PLAN §7) were updated; if not, list what is missing
     as the first step of the next phase.
   - Commit: `checkpoint(<phase>): passed` with a `Gaps:` line in the body if any.
5. On a clear fail, don't close the phase. Log the attempt in `checkpoints.md`, point to what to
   review, and suggest retrying next session — spaced retrieval beats immediate retry.

## Tone

Colleague, not examiner. Short questions, honest verdicts, zero condescension.
