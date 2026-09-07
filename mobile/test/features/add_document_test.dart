import 'dart:typed_data';

import 'package:flutter/material.dart';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';
import 'package:wathiq_mobile/core/auth/token_store.dart';
import 'package:wathiq_mobile/core/models/document.dart';
import 'package:wathiq_mobile/features/documents/create_document.dart';
import 'package:wathiq_mobile/features/documents/documents_providers.dart';
import 'package:wathiq_mobile/features/documents/image_capture.dart';
import 'package:wathiq_mobile/features/guides/guides_providers.dart';

import '../helpers.dart';

/// The camera faked at the WALL: bytes injected where the platform channel
/// would be - everything beneath (preview, draft assembly, submit, redirect)
/// is real code under test.
class FakeCapture implements ImageCapture {
  CapturedImage? next;
  @override
  Future<CapturedImage?> pick(CaptureSource source) async => next;
}

final _passportType = DocumentType(
    id: 't1', code: 'PASSPORT', nameAr: 'جواز السفر', nameEn: 'Passport');

void main() {
  late FakeCapture capture;
  late List<NewDocumentDraft> submitted;

  Future<void> pumpForm(WidgetTester tester, {bool failFirst = false}) async {
    capture = FakeCapture();
    submitted = [];
    var calls = 0;
    await tester.pumpWidget(ProviderScope(
      overrides: [
        tokenStoreProvider.overrideWithValue(signedInStore()),
        guidesListProvider.overrideWith((ref) => Future.value(const [])),
        documentsListProvider.overrideWith((ref) => Future.value(const [])),
        documentTypesProvider
            .overrideWith((ref) => Future.value({'t1': _passportType})),
        documentDetailProvider.overrideWith((ref, id) => Future.value(
            DocumentModel(
                id: id,
                holderId: 'h1',
                documentTypeId: 't1',
                status: DocumentStatus.active))),
        imageCaptureProvider.overrideWithValue(capture),
        createDocumentSenderProvider.overrideWithValue((draft) async {
          calls++;
          if (failFirst && calls == 1) {
            throw Exception('down');
          }
          submitted.add(draft);
          return 'new-doc-id';
        }),
      ],
      child: const WathiqApp(),
    ));
    await tester.pumpAndSettle();
    await tester.tap(find.text('الوثائق'));
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('إضافة وثيقة')); // the FAB
    await tester.pumpAndSettle();
  }

  testWidgets('capture → preview → save submits the draft and shows the detail',
      (tester) async {
    await pumpForm(tester);

    // Save is disabled until the one required field (type) is chosen.
    await tester.tap(find.text('حفظ'));
    await tester.pumpAndSettle();
    expect(submitted, isEmpty);

    await tester.tap(find.text('جواز السفر'));
    await tester.enterText(find.byType(TextField), 'P-102030');

    capture.next = CapturedImage(
        bytes: Uint8List.fromList(List.filled(3000, 7)),
        fileName: 'scan.jpg',
        mimeType: 'image/jpeg');
    await tester.tap(find.text('التقاط صورة'));
    await tester.pumpAndSettle();
    expect(find.text('الصورة جاهزة للرفع'), findsOneWidget); // preview state

    await tester.tap(find.text('حفظ'));
    await tester.pumpAndSettle();

    final draft = submitted.single;
    expect(draft.documentTypeId, 't1');
    expect(draft.number, 'P-102030');
    expect(draft.image!.fileName, 'scan.jpg');
    expect(find.text('تاريخ الانتهاء'), findsOneWidget); // the detail replaced the form
  });

  testWidgets('a failed save keeps the form and its data for a free retry',
      (tester) async {
    await pumpForm(tester, failFirst: true);
    await tester.tap(find.text('جواز السفر'));
    await tester.enterText(find.byType(TextField), 'P-1');

    await tester.tap(find.text('حفظ'));
    await tester.pumpAndSettle();

    expect(find.textContaining('تعذّر الحفظ'), findsOneWidget);
    expect(find.text('P-1'), findsOneWidget); // nothing lost

    await tester.tap(find.text('حفظ')); // retry succeeds
    await tester.pumpAndSettle();
    expect(submitted.single.number, 'P-1');
  });

  testWidgets('cancelling the picker changes nothing', (tester) async {
    await pumpForm(tester);
    capture.next = null; // user backed out of the OS sheet

    await tester.tap(find.text('من المعرض'));
    await tester.pumpAndSettle();

    expect(find.text('التقاط صورة'), findsOneWidget); // buttons still offered
  });
}
