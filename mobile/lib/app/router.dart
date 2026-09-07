import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../features/documents/add_document_page.dart';
import '../features/documents/document_detail_page.dart';
import '../features/guides/guide_detail_page.dart';
import 'home_shell.dart';

/// Routes as DATA - the Angular route table, not imperative pushes. The URL is
/// the source of truth: /guides/renew-passport deep-links straight into the
/// detail, and `state.pathParameters` is withComponentInputBinding by hand.
/// A PROVIDER, not a global: a GoRouter carries navigation STATE, and a
/// global one leaks the last test's location into the next (found the hard
/// way) - per-container also opens the door to auth-based redirects later.
final routerProvider = Provider<GoRouter>((ref) => GoRouter(
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) => const HomeShell(),
      routes: [
        GoRoute(
          path: 'guides/:slug',
          builder: (context, state) =>
              GuideDetailPage(slug: state.pathParameters['slug']!),
        ),
        GoRoute(
          // literal BEFORE the param (5.7's route-order lesson, third stack).
          path: 'documents/new',
          builder: (context, state) => const AddDocumentPage(),
        ),
        GoRoute(
          path: 'documents/:id',
          builder: (context, state) =>
              DocumentDetailPage(id: state.pathParameters['id']!),
        ),
      ],
    ),
  ],
));
