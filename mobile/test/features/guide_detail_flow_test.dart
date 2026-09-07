import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';
import 'package:wathiq_mobile/core/models/guide.dart';
import 'package:wathiq_mobile/features/guides/guide_detail_providers.dart';
import 'package:wathiq_mobile/features/guides/guides_providers.dart';

const _summary = GuideSummary(
  id: 'g1',
  slug: 'renew-passport',
  titleAr: 'تجديد جواز السفر',
  titleEn: 'Renew a passport',
);

final _detail = GuideDetail(
  id: 'g1',
  slug: 'renew-passport',
  titleAr: 'تجديد جواز السفر',
  titleEn: 'Renew a passport',
  version: GuideVersion(
    id: 'v1',
    versionNo: 1,
    language: 'ar',
    bodyMarkdown: '## قبل أن تبدأ\nالتجديد إلكتروني بالكامل.',
    lastVerifiedAt: DateTime(2026, 9, 1),
    steps: const ['سدّد الرسوم', 'قدّم الطلب'],
    fees: '300 ريال',
  ),
);

void main() {
  testWidgets('tap a guide → deep-linked detail → flag it outdated',
      (tester) async {
    final flagged = <String>[];

    await tester.pumpWidget(ProviderScope(
      overrides: [
        guidesListProvider.overrideWith((ref) => Future.value([_summary])),
        // Family override: (ref, arg) - one fake serves every slug.
        guideDetailProvider
            .overrideWith((ref, slug) => Future.value(_detail)),
        // The seam records instead of POSTing - FakeGuideAnswerer's idea here.
        outdatedFlagSenderProvider
            .overrideWith((ref) => (versionId) async => flagged.add(versionId)),
      ],
      child: const WathiqApp(),
    ));
    await tester.pumpAndSettle();

    // The ROUTE does the navigation: tapping pushes /guides/renew-passport.
    await tester.tap(find.text('تجديد جواز السفر'));
    await tester.pumpAndSettle();

    expect(find.textContaining('آخر تحقق:'), findsOneWidget); // freshness first
    expect(find.text('300 ريال'), findsOneWidget); // facts card
    expect(find.text('قبل أن تبدأ'), findsOneWidget); // parsed heading
    expect(find.text('قدّم الطلب'), findsOneWidget); // numbered step

    await tester.tap(find.text('هل المعلومات قديمة؟ أبلغنا'));
    await tester.pumpAndSettle();

    expect(flagged, ['v1']); // the version id, not the guide - the 5.6 anchor
    expect(find.textContaining('شكرًا لك'), findsOneWidget);

    // Back pops to the list (push, not go). By TYPE, not pageBack(): the
    // Material back button's tooltip is localized (ar) and pageBack misses it.
    await tester.tap(find.byType(BackButton));
    await tester.pumpAndSettle();
    expect(find.byType(ListTile), findsOneWidget);
  });
}
