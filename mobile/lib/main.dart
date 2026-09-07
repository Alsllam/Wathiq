import 'package:flutter/material.dart';

import 'app/app.dart';

/// The entire bootstrap: one function handing ONE widget to the engine.
/// Angular's analogue is bootstrapApplication(AppComponent) - but there is no
/// separate template/style/router config file waiting anywhere: everything the
/// app is or does will be widgets composed under this root.
void main() {
  runApp(const WathiqApp());
}
