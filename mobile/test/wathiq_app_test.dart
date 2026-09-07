import 'package:flutter_test/flutter_test.dart';
import 'package:wathiq_mobile/app/app.dart';

/// A widget test runs on the Dart VM against a headless renderer - no device,
/// no emulator (why every Phase 6 step can gate green in the container). The
/// tester pumps a widget tree and queries it with finders, the way portal
/// specs query the DOM after detectChanges.
void main() {
  testWidgets('the shell boots and shows the app identity', (tester) async {
    await tester.pumpWidget(const WathiqApp());

    expect(find.text('وثيق — Wathiq'), findsOneWidget);
  });
}
