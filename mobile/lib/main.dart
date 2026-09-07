import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'app/app.dart';

/// ProviderScope is the container every `ref` resolves against - the
/// application injector at the very root (and, in tests, the seam where
/// overrides swap real providers for fakes).
void main() {
  runApp(const ProviderScope(child: WathiqApp()));
}
