import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/features/guides/guide_body.dart';

void main() {
  test('splits headings, joined paragraphs and bullets - as data, not markup',
      () {
    final segments = parseGuideBody(
        '## قبل أن تبدأ\nالتجديد إلكتروني\nبالكامل عبر أبشر.\n\n- يشترط عنوان وطني\n\nنص ختامي.');

    expect(segments, hasLength(4));
    expect(segments[0], isA<HeadingSegment>());
    expect(segments[0].text, 'قبل أن تبدأ');
    // hard-wrapped source lines join into one paragraph
    expect(segments[1].text, 'التجديد إلكتروني بالكامل عبر أبشر.');
    expect(segments[2], isA<BulletSegment>());
    expect(segments[3].text, 'نص ختامي.');
  });

  test('empty and heading-only bodies', () {
    expect(parseGuideBody(''), isEmpty);
    expect(parseGuideBody('### Fees').single, isA<HeadingSegment>());
  });
}
