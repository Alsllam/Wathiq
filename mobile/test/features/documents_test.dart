import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';
import 'package:wathiq_mobile/core/auth/token_store.dart';
import 'package:wathiq_mobile/core/models/document.dart';
import 'package:wathiq_mobile/features/documents/documents_providers.dart';
import 'package:wathiq_mobile/features/documents/expiry.dart';
import 'package:wathiq_mobile/features/guides/guides_providers.dart';

import '../helpers.dart';

final _passportType = DocumentType(
  id: 't1',
  code: 'PASSPORT',
  nameAr: 'جواز السفر',
  nameEn: 'Passport',
);

DocumentModel _doc({int? days, DateTime? expiry}) => DocumentModel(
      id: 'd1',
      holderId: 'h1',
      documentTypeId: 't1',
      status: DocumentStatus.active,
      number: 'P-102030',
      expiryDate: expiry,
      daysUntilExpiry: days,
      attachments: const [
        AttachmentModel(id: 'a1', mimeType: 'image/png', sizeBytes: 2048),
      ],
    );

Future<void> _pump(WidgetTester tester,
    {required TokenStore store, required List<DocumentModel> docs}) async {
  await tester.pumpWidget(ProviderScope(
    overrides: [
      tokenStoreProvider.overrideWithValue(store),
      guidesListProvider.overrideWith((ref) => Future.value(const [])),
      documentTypesProvider
          .overrideWith((ref) => Future.value({'t1': _passportType})),
      documentsListProvider.overrideWith((ref) => Future.value(docs)),
      documentDetailProvider
          .overrideWith((ref, id) => Future.value(docs.first)),
    ],
    child: const WathiqApp(),
  ));
  await tester.pumpAndSettle();
  await tester.tap(find.text('الوثائق')); // the documents tab
  await tester.pumpAndSettle();
}

void main() {
  group('expirySeverity (pure)', () {
    test('maps days to severities, null is calm', () {
      expect(expirySeverity(null), ExpirySeverity.none);
      expect(expirySeverity(-1), ExpirySeverity.expired);
      expect(expirySeverity(0), ExpirySeverity.soon);
      expect(expirySeverity(30), ExpirySeverity.soon);
      expect(expirySeverity(31), ExpirySeverity.ok);
    });
  });

  testWidgets('signed out, the tab invites sign-in instead of erroring',
      (tester) async {
    await _pump(tester, store: InMemoryTokenStore(), docs: const []);

    expect(find.textContaining('سجّل الدخول لعرض وثائقك'), findsOneWidget);
  });

  testWidgets('signed in, rows show localized type names and expiry chips',
      (tester) async {
    await _pump(tester, store: signedInStore(), docs: [_doc(days: 7)]);

    expect(find.text('جواز السفر'), findsOneWidget); // type name, ar first
    expect(find.text('P-102030'), findsOneWidget);
    expect(find.text('خلال 7 يومًا'), findsOneWidget); // the soon chip
  });

  testWidgets('tapping a row deep-links to the detail with attachments',
      (tester) async {
    await _pump(tester, store: signedInStore(), docs: [_doc(days: 7)]);

    await tester.tap(find.text('P-102030'));
    await tester.pumpAndSettle();

    expect(find.text('تاريخ الانتهاء'), findsOneWidget);
    expect(find.text('image/png'), findsOneWidget); // the attachment row
    expect(find.text('2 KB'), findsOneWidget);
  });
}
