import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';
import 'package:wathiq_mobile/core/api/api_client.dart';
import 'package:wathiq_mobile/core/locale_provider.dart';
import 'package:wathiq_mobile/core/models/guide.dart';
import 'package:wathiq_mobile/features/guides/guides_providers.dart';

const _passport = GuideSummary(
  id: 'g1',
  slug: 'renew-passport',
  titleAr: 'تجديد جواز السفر',
  titleEn: 'Renew a passport',
);

/// Overrides are Riverpod's TestBed.configureTestingModule: the SAME widget
/// tree runs against fakes, swapped at the ProviderScope seam - no HTTP, no
/// Dio mocks, just "this provider resolves to that value here". (ProviderScope
/// is built inline because flutter_riverpod 3.x doesn't export the Override
/// type for a helper's signature - inference at the parameter names it.)
void main() {
  testWidgets('renders guides Arabic-first, flipping with the locale',
      (tester) async {
    await tester.pumpWidget(ProviderScope(
      overrides: [
        guidesListProvider.overrideWith((ref) => Future.value([_passport])),
      ],
      child: const WathiqApp(),
    ));
    await tester.pumpAndSettle();

    // ar: Arabic title leads, English is the subtitle.
    final tile = tester.widget<ListTile>(find.byType(ListTile));
    expect((tile.title! as Text).data, 'تجديد جواز السفر');

    await tester.tap(find.text('English'));
    await tester.pumpAndSettle();

    final flipped = tester.widget<ListTile>(find.byType(ListTile));
    expect((flipped.title! as Text).data, 'Renew a passport');
  });

  testWidgets('a failed load shows the honest error state with retry',
      (tester) async {
    await tester.pumpWidget(ProviderScope(
      overrides: [
        guidesListProvider.overrideWith(
            (ref) => Future<List<GuideSummary>>.error(Exception('down'))),
      ],
      child: const WathiqApp(),
    ));
    await tester.pumpAndSettle();

    expect(find.text('تعذّر التحميل — تحقق من الاتصال'), findsOneWidget);
    expect(find.text('إعادة المحاولة'), findsOneWidget);
  });

  test('the Dio client follows the app language - a dependency edge, not luck',
      () {
    final container = ProviderContainer();
    addTearDown(container.dispose);

    expect(
        container.read(dioProvider).options.headers['Accept-Language'], 'ar');

    container.read(localeProvider.notifier).toggle();

    // dioProvider WATCHES localeProvider: the toggle invalidated it, and the
    // next read rebuilds the client with the new header (5.7's bug, prevented
    // by construction and provable in a plain unit test).
    expect(
        container.read(dioProvider).options.headers['Accept-Language'], 'en');
  });
}
