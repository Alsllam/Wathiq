/**
 * Markdown-lite for guide bodies, as DATA not HTML: the authored content uses only headings and
 * paragraphs (see the seeded guide), so a full markdown pipe - and the innerHTML sanitization
 * questions it drags in - is not earned yet. The template renders segments with @for/@switch;
 * nothing ever reaches the DOM as markup.
 */
export interface GuideBodySegment {
  kind: 'heading' | 'paragraph' | 'bullet';
  text: string;
}

export function parseGuideBody(markdown: string): GuideBodySegment[] {
  const segments: GuideBodySegment[] = [];
  let paragraph: string[] = [];

  const flush = () => {
    if (paragraph.length) {
      segments.push({ kind: 'paragraph', text: paragraph.join(' ') });
      paragraph = [];
    }
  };

  for (const raw of markdown.replace(/\r\n/g, '\n').split('\n')) {
    const line = raw.trim();
    if (!line) {
      flush();
    } else if (/^#{1,6}\s/.test(line)) {
      flush();
      segments.push({ kind: 'heading', text: line.replace(/^#{1,6}\s+/, '') });
    } else if (/^[-*]\s/.test(line)) {
      flush();
      segments.push({ kind: 'bullet', text: line.replace(/^[-*]\s+/, '') });
    } else {
      paragraph.push(line);   // hard-wrapped source lines join into one paragraph
    }
  }
  flush();

  return segments;
}
