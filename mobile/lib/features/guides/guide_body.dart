/// Markdown-lite as DATA (5.7's parser ported to Dart): the authored bodies
/// use only headings/paragraphs/bullets, so a full markdown package - and its
/// HTML questions - stays unearned. The widget renders segments; no markup
/// ever reaches the tree.
sealed class GuideBodySegment {
  const GuideBodySegment(this.text);
  final String text;
}

class HeadingSegment extends GuideBodySegment {
  const HeadingSegment(super.text);
}

class ParagraphSegment extends GuideBodySegment {
  const ParagraphSegment(super.text);
}

class BulletSegment extends GuideBodySegment {
  const BulletSegment(super.text);
}

List<GuideBodySegment> parseGuideBody(String markdown) {
  final segments = <GuideBodySegment>[];
  final paragraph = <String>[];

  void flush() {
    if (paragraph.isNotEmpty) {
      segments.add(ParagraphSegment(paragraph.join(' ')));
      paragraph.clear();
    }
  }

  for (final raw in markdown.replaceAll('\r\n', '\n').split('\n')) {
    final line = raw.trim();
    if (line.isEmpty) {
      flush();
    } else if (RegExp(r'^#{1,6}\s').hasMatch(line)) {
      flush();
      segments.add(HeadingSegment(line.replaceFirst(RegExp(r'^#{1,6}\s+'), '')));
    } else if (RegExp(r'^[-*]\s').hasMatch(line)) {
      flush();
      segments.add(BulletSegment(line.replaceFirst(RegExp(r'^[-*]\s+'), '')));
    } else {
      paragraph.add(line); // hard-wrapped source lines join into one paragraph
    }
  }
  flush();

  return segments;
}
