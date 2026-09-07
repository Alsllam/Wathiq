import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';
import 'package:wathiq_mobile/features/guides/guides_providers.dart';

/// Shell tests: Arabic-first with RTL FROM THE LOCALE, and the toggle
/// re-renders the whole tree. The guides tab is live since 6.4, so its
/// provider is overridden to keep the shell deterministic (no network).
void main() {
  Widget app() => ProviderScope(
        overrides: [
          guidesListProvider.overrideWith((ref) => Future.value(const [])),
        ],
        child: const WathiqApp(),
      );

  testWidgets('boots Arabic-first with RTL flowing from the locale',
      (tester) async {
    await tester.pumpWidget(app());
    await tester.pumpAndSettle();

    expect(find.text('وثيق'), findsOneWidget); // AppBar title
    expect(find.text('الأدلة'), findsOneWidget); // nav label

    // The one-line proof of RTL: the context under MaterialApp reports rtl
    // because the LOCALE is ar - nobody set a direction anywhere.
    final context = tester.element(find.byType(Scaffold).first);
    expect(Directionality.of(context), TextDirection.rtl);
  });

  testWidgets('the language toggle flips text AND direction together',
      (tester) async {
    await tester.pumpWidget(app());
    await tester.pumpAndSettle();

    await tester.tap(find.text('English'));
    await tester.pumpAndSettle();

    expect(find.text('Wathiq'), findsOneWidget);
    expect(find.text('Guides'), findsOneWidget);
    final context = tester.element(find.byType(Scaffold).first);
    expect(Directionality.of(context), TextDirection.ltr);
  });

  testWidgets('bottom navigation switches tabs', (tester) async {
    await tester.pumpWidget(app());
    await tester.pumpAndSettle();

    await tester.tap(find.text('التذكيرات'));
    await tester.pumpAndSettle();

    expect(find.text('تُبنى في الخطوة 6.9'), findsOneWidget);
  });
}
