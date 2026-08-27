# Deliverable documents

Markdown sources rendered to `.docx` with `/make-doc <key>` (Pandoc + reference template in
`_template/`). Keys: `vision`, `srs`, `architecture`, `database`, `api`, `ai-safety`, `privacy`,
`user-guide`, `test-plan`. Rendered files go to `docs/deliverables/out/` (committed, so the Word
versions travel with the repo).

Each document has a front-matter block (title, version, date, status) and a revision table.

## Building

```powershell
.\docs\deliverables\_template\build.ps1 srs     # one document
.\docs\deliverables\_template\build.ps1 all     # every *.md here except README
```

- `_template/reference.docx` is the style sheet Pandoc copies (fonts, headings, `RTL` style,
  table borders). It is generated — edit `_template/make-reference.py` and re-run it, never the
  `.docx` by hand.
- Arabic paragraphs: wrap in `::: {custom-style="RTL"}` … `:::`.
- Front matter `title`/`subtitle`/`author`/`date` render on the title page; `version`/`status`
  are metadata only — the revision-history table is the visible version record.
- `smoke.md` is the pipeline's smoke test; re-render it after touching the template.
