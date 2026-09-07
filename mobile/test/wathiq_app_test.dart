import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';

/// Shell tests: Arabic-first with RTL FROM THE LOCALE (no manual mirroring to
/// test - the assertion IS Directionality.of), and the toggle re-renders the
/// whole tree in the other language.
void main() {
  testWidgets('boots Arabic-first with RTL flowing from the locale',
      (tester) async {
    await tester.pumpWidget(const WathiqApp());
    await tester.pumpAndSettle(); // let the localization delegates load

    expect(find.text('وثيق'), findsOneWidget); // AppBar title
    expect(find.text('الأدلة'), findsWidgets); // nav + first tab body

    // The one-line proof of RTL: the context under MaterialApp reports rtl
    // because the LOCALE is ar - nobody set a direction anywhere.
    final context = tester.element(find.byType(Scaffold));
    expect(Directionality.of(context), TextDirection.rtl);
  });

  testWidgets('the language toggle flips text AND direction together',
      (tester) async {
    await tester.pumpWidget(const WathiqApp());
    await tester.pumpAndSettle();

    await tester.tap(find.text('English'));
    await tester.pumpAndSettle();

    expect(find.text('Wathiq'), findsOneWidget);
    expect(find.text('Guides'), findsWidgets);
    final context = tester.element(find.byType(Scaffold));
    expect(Directionality.of(context), TextDirection.ltr);
  });

  testWidgets('bottom navigation switches tabs', (tester) async {
    await tester.pumpWidget(const WathiqApp());
    await tester.pumpAndSettle();

    await tester.tap(find.text('التذكيرات'));
    await tester.pumpAndSettle();

    expect(find.text('تُبنى في الخطوة 6.9'), findsOneWidget);
  });
}
