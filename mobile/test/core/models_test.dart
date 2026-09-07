import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/core/models/document.dart';
import 'package:wathiq_mobile/core/models/guide.dart';
import 'package:wathiq_mobile/core/models/guide_chat.dart';
import 'package:wathiq_mobile/core/models/list_result.dart';
import 'package:wathiq_mobile/core/models/reminder.dart';

/// Every payload here was CAPTURED from the running API during Phase 5's live
/// checks (5.2 guide detail, 5.5 chat refusal) or mirrors the swagger shapes -
/// the models are verified against reality, not against memory of it.
void main() {
  group('GuideDetail', () {
    // Recorded 5.2: GET /api/guides/guide/by-slug?slug=renew-passport&language=ar
    const captured = '''
    {"id":"g-1","slug":"renew-passport","titleAr":"تجديد جواز السفر","titleEn":"Renew a passport",
     "version":{"id":"v-1","guideId":"g-1","versionNo":1,"language":"ar",
       "bodyMarkdown":"## قبل أن تبدأ\\nالتجديد إلكتروني بالكامل.",
       "requiredDocuments":"الهوية الوطنية سارية المفعول","fees":"300 ريال (5 سنوات) أو 600 ريال (10 سنوات)",
       "location":"منصة أبشر (absher.sa)","lastVerifiedAt":"2026-09-01",
       "publishedAt":"2026-09-03T11:49:26.887Z",
       "steps":["سدّد رسوم تجديد الجواز.","سجّل الدخول إلى أبشر."]}}''';

    test('decodes the real Arabic payload, dates included', () {
      final detail =
          GuideDetail.fromJson(jsonDecode(captured) as Map<String, dynamic>);

      expect(detail.slug, 'renew-passport');
      expect(detail.titleAr, 'تجديد جواز السفر');
      expect(detail.version.lastVerifiedAt, DateTime(2026, 9, 1));
      expect(detail.version.steps, hasLength(2));
      expect(detail.version.fees, contains('300'));
    });
  });

  group('ListResult', () {
    test('decodes the ABP envelope generically', () {
      final list = ListResult<GuideSummary>.fromJson(
        jsonDecode('{"items":[{"id":"1","slug":"renew-passport",'
                '"titleAr":"تجديد جواز السفر","titleEn":"Renew a passport"}]}')
            as Map<String, dynamic>,
        GuideSummary.fromJson,
      );

      expect(list.items.single.slug, 'renew-passport');
    });
  });

  group('GuideChatResponse', () {
    // Recorded 5.5, verbatim: the honest refusal on a model-less install.
    const refusal = '''
    {"answered":false,"answer":null,
     "message":"لم أجد إجابة موثوقة في الأدلة المنشورة. تصفّح قائمة الأدلة من فضلك — الإجابة من خارج الأدلة قد تكون خاطئة.",
     "citations":[],"lastVerifiedAt":null,"hallucinatedCitationsDropped":false}''';

    test('the refusal contract: nulls are data, not errors', () {
      final r = GuideChatResponse.fromJson(
          jsonDecode(refusal) as Map<String, dynamic>);

      expect(r.answered, isFalse);
      expect(r.answer, isNull); // String? holds the absence soundly
      expect(r.message, contains('لم أجد إجابة موثوقة'));
      expect(r.citations, isEmpty);
      expect(r.lastVerifiedAt, isNull);
    });

    test('a grounded answer carries citations with freshness', () {
      final r = GuideChatResponse.fromJson(jsonDecode('''
        {"answered":true,"answer":"الرسوم 300 ريال.","citations":[
          {"chunkId":"c1","guideVersionId":"v-1","guideSlug":"renew-passport",
           "titleAr":"تجديد جواز السفر","titleEn":"Renew a passport",
           "snippet":"300 ريال","lastVerifiedAt":"2026-09-01"}],
         "lastVerifiedAt":"2026-09-01","hallucinatedCitationsDropped":true}''')
          as Map<String, dynamic>);

      expect(r.citations.single.guideSlug, 'renew-passport');
      expect(r.hallucinatedCitationsDropped, isTrue);
      expect(GuideChatRequest(question: 'كم الرسوم؟').toJson(),
          {'question': 'كم الرسوم؟'});
    });
  });

  group('DocumentModel', () {
    test('all-optional fields absent stay null - no defaults invented', () {
      final doc = DocumentModel.fromJson(jsonDecode(
              '{"id":"d1","holderId":"h1","documentTypeId":"t1","status":0}')
          as Map<String, dynamic>);

      expect(doc.status, DocumentStatus.active);
      expect(doc.expiryDate, isNull); // the screen must design this state
      expect(doc.number, isNull);
    });

    test('wire ints map through the enhanced enums', () {
      expect(DocumentStatus.fromWire(1), DocumentStatus.archived);
      expect(ReminderStatus.fromWire(3), ReminderStatus.cancelled);
    });
  });

  group('Reminder', () {
    test('sent history keeps its timestamp, pending has none', () {
      final sent = Reminder.fromJson(jsonDecode(
              '{"id":"r1","documentId":"d1","offsetDays":7,"dueDate":"2026-08-28",'
              '"expiryDate":"2026-09-04","status":1,"sentAt":"2026-08-28T03:00:11Z"}')
          as Map<String, dynamic>);

      expect(sent.status, ReminderStatus.sent);
      expect(sent.sentAt, isNotNull);
      expect(sent.expiryDate.difference(sent.dueDate).inDays, 7);
    });
  });
}
