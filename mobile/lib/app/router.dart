import 'package:go_router/go_router.dart';

import '../features/documents/document_detail_page.dart';
import '../features/guides/guide_detail_page.dart';
import 'home_shell.dart';

/// Routes as DATA - the Angular route table, not imperative pushes. The URL is
/// the source of truth: /guides/renew-passport deep-links straight into the
/// detail, and `state.pathParameters` is withComponentInputBinding by hand.
final router = GoRouter(
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
          path: 'documents/:id',
          builder: (context, state) =>
              DocumentDetailPage(id: state.pathParameters['id']!),
        ),
      ],
    ),
  ],
);
