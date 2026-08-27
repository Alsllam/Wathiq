---
name: next-step
description: Execute the next unchecked step in ROADMAP.md - implement it in teaching mode, write its learning doc, update deliverable docs if the step touches them, and commit it as a single step commit. Use when the user says "next step", "continue", or invokes /next-step.
---

# next-step

Execute exactly **one** roadmap step end-to-end. Never two, unless the user explicitly asks.

## Procedure

1. **Locate the step.** Read `ROADMAP.md`; the target is the first unchecked `[ ]` step of the
   phase marked `active`. If it is a phase-expansion step (`N.0`), the work is to break the phase
   into commit-sized steps in ROADMAP.md (using `docs/PLAN.md` §3–§5 as the source), each with a
   *Topics* line and, where relevant, a *Docs* line naming the deliverable it updates. If the next
   item is a checkpoint (`N.CP`), stop and tell the user to run `/checkpoint`.
2. **Announce the lesson first.** Before writing code, state in 3–6 lines: what will be built,
   which topics it teaches (from the step's *Topics* line), and what the developer already knows
   that it maps to (Angular / ABP / SQL). For Flutter steps, name the single new Dart/Flutter
   concept being introduced. When the step mirrors something in a reference repo
   (`d:\Projects\MoD.HousingProject.Frontend`, `D:\Projects\MoD.PoC.HousingProject`), read the
   original first and say what is being reused vs. done differently.
3. **Implement in teaching mode.**
   - Small, readable diffs. Comment the *why* at the exact line where a new idea appears
     (one short comment, not an essay).
   - Obey every guardrail in CLAUDE.md (privacy routing, AI behind interfaces, module
     boundaries, RTL-safe classes, no secrets).
   - Add or update at least one test when the step has a test rig.
   - Verify with the narrowest relevant command (`dotnet build/test`, `nx test`, `flutter test`,
     `pandoc …`) and show the result. Never claim green without running it.
4. **Update deliverable docs** if the step's *Docs* line names one (e.g. a new entity → the
   `database.md` dictionary; a new endpoint → `api.md`). Re-render with the make-doc script so
   `out/*.docx` stays in sync.
5. **Write the learning doc** at `docs/learning/<step-id>-<slug>.md` following
   `docs/learning/TEMPLATE.md`. "The mental shift" must be specific to what was built; "Gotchas
   hit" must be real ones from this session.
6. **Tick the checkbox** in ROADMAP.md.
7. **Commit** everything from this step as one commit:

   ```
   step(<id>): <imperative subject>

   Learned: <comma-separated topic list>
   ```

   Stage only files belonging to this step. Do not push (local-only repo).
8. **Hand back.** Name the step, the commit hash, the learning doc path, and what the *next*
   step will be — then stop. Do not start the next step.

## Quality bar

- The learning doc's table must reference real files/lines from this commit.
- If the step turns out bigger than one sitting, split it: tick nothing, propose sub-steps
  (`<id>a`, `<id>b`) in ROADMAP.md, commit the roadmap change, and do the first sub-step.
- Prefer the free/local path (Ollama, LocalDB, smtp4dev) over anything that needs a paid key.
