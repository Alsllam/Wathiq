"""Render Mermaid fences in a deliverable to PNGs Pandoc can embed.

Convention (see architecture.md): an image line immediately precedes its Mermaid source —

    ![System context](assets/c4-context.png)
    ```mermaid
    ...
    ```

The Markdown keeps the source (diffable, reviewable); the PNG is what Word shows.
Usage:  python render-diagrams.py architecture   (from any cwd)
Requires Node (uses `npx @mermaid-js/mermaid-cli`; first run downloads Chromium).
"""
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent          # docs/deliverables
TMP = ROOT / ".tmp"
TMP.mkdir(exist_ok=True)

PATTERN = re.compile(r"!\[[^\]]*\]\((assets/[^)]+\.png)\)\s*\n```mermaid\n(.*?)\n```", re.S)

for key in sys.argv[1:]:
    text = (ROOT / f"{key}.md").read_text(encoding="utf-8")
    for out_rel, source in PATTERN.findall(text):
        out = ROOT / out_rel
        mmd = TMP / (out.stem + ".mmd")
        mmd.write_text(source, encoding="utf-8")
        # -b white: transparent PNGs look black in Word's dark-mode preview; -s 2: crisp at print size
        subprocess.run(
            ["npx", "-y", "@mermaid-js/mermaid-cli", "-i", str(mmd), "-o", str(out), "-b", "white", "-s", "2"],
            check=True, shell=True,
        )
        print(f"OK  {key}: {out_rel} ({out.stat().st_size // 1024} KB)")
