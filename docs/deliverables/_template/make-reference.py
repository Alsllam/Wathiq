"""Build reference.docx — the *style sheet* Pandoc copies into every deliverable.

Pandoc only takes styles (fonts, colours, spacing) from the reference file; content is ignored.
Think of it as theme.scss for Word. Re-run after editing:  python make-reference.py
"""
import subprocess
from docx import Document
from docx.enum.style import WD_STYLE_TYPE
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Pt, RGBColor

LATIN = "Calibri"     # body text for English
ARABIC = "Arial"      # cs (complex-script) font: Arial ships with Windows and has full Arabic glyphs
ACCENT = RGBColor(0x1F, 0x4E, 0x79)

# 1. Start from Pandoc's own default so every style Pandoc emits exists.
subprocess.run(["pandoc", "-o", "reference.docx", "--print-default-data-file", "reference.docx"], check=True)
doc = Document("reference.docx")


def set_fonts(style, size=None, bold=None, color=None):
    style.font.name = LATIN
    if size: style.font.size = Pt(size)
    if bold is not None: style.font.bold = bold
    if color: style.font.color.rgb = color
    rpr = style.element.get_or_add_rPr()
    rfonts = rpr.find(qn("w:rFonts"))
    if rfonts is None:
        rfonts = OxmlElement("w:rFonts"); rpr.append(rfonts)
    # Word picks the font per script: ascii/hAnsi for Latin, cs for Arabic. Both must be set.
    for attr in ("w:ascii", "w:hAnsi", "w:eastAsia"):
        rfonts.set(qn(attr), LATIN)
    rfonts.set(qn("w:cs"), ARABIC)


for name in ("Normal", "Body Text", "First Paragraph", "Compact"):
    set_fonts(doc.styles[name], size=11)
set_fonts(doc.styles["Title"], size=26, bold=True, color=ACCENT)
set_fonts(doc.styles["Subtitle"], size=16, color=ACCENT)
for level, size in ((1, 18), (2, 14), (3, 12), (4, 11)):
    set_fonts(doc.styles[f"Heading {level}"], size=size, bold=True, color=ACCENT)

# 2. RTL paragraph style — used from Markdown as  ::: {custom-style="RTL"}  ...  :::
rtl = doc.styles.add_style("RTL", WD_STYLE_TYPE.PARAGRAPH)
rtl.base_style = doc.styles["Body Text"]
rtl.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.RIGHT
set_fonts(rtl, size=12)
ppr = rtl.element.get_or_add_pPr()
ppr.append(OxmlElement("w:bidi"))          # paragraph direction = right-to-left (Word's dir="rtl")
rpr = rtl.element.get_or_add_rPr()
rpr.append(OxmlElement("w:rtl"))           # run direction, so punctuation sits on the correct side

# 3. Tables: Pandoc uses the "Table" style; give it borders and a shaded header row.
tbl = doc.styles["Table"]
tpr = tbl.element.find(qn("w:tblPr"))
if tpr is None:
    tpr = OxmlElement("w:tblPr"); tbl.element.append(tpr)
borders = OxmlElement("w:tblBorders")
for edge in ("top", "left", "bottom", "right", "insideH", "insideV"):
    b = OxmlElement(f"w:{edge}")
    b.set(qn("w:val"), "single"); b.set(qn("w:sz"), "4"); b.set(qn("w:color"), "BFBFBF")
    borders.append(b)
tpr.append(borders)

doc.save("reference.docx")
print("reference.docx written; styles:", ", ".join(s.name for s in doc.styles if s.name in ("RTL", "Table")))
