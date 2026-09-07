import { parseGuideBody } from './guide-body';

describe('parseGuideBody', () => {
  it('splits headings, paragraphs and bullets without emitting markup', () => {
    const segments = parseGuideBody(
      '## قبل أن تبدأ\nالتجديد إلكتروني\nبالكامل عبر أبشر.\n\n- يشترط عنوان وطني\n\nنص ختامي.'
    );

    expect(segments).toEqual([
      { kind: 'heading', text: 'قبل أن تبدأ' },
      // hard-wrapped source lines join into one paragraph
      { kind: 'paragraph', text: 'التجديد إلكتروني بالكامل عبر أبشر.' },
      { kind: 'bullet', text: 'يشترط عنوان وطني' },
      { kind: 'paragraph', text: 'نص ختامي.' },
    ]);
  });

  it('handles empty and heading-only bodies', () => {
    expect(parseGuideBody('')).toEqual([]);
    expect(parseGuideBody('### Fees')).toEqual([{ kind: 'heading', text: 'Fees' }]);
  });
});
