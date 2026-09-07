import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import '../../core/locale_provider.dart';
import '../../core/models/guide.dart';

/// A FAMILY is a parameterized provider: one cache entry per slug - the bridge
/// from a route's path param to data, like withComponentInputBinding feeding
/// input.required into httpResource (5.7). It watches the locale too, so the
/// language toggle refetches the right published version - same two-signal
/// URL as the portal's detail resource.
final guideDetailProvider =
    FutureProvider.family<GuideDetail, String>((ref, slug) async {
  final dio = ref.watch(dioProvider);
  final language = ref.watch(localeProvider).languageCode;
  final response = await dio.get<Map<String, dynamic>>(
    '/api/guides/guide/by-slug',
    queryParameters: {'slug': slug, 'language': language},
  );
  return GuideDetail.fromJson(response.data!);
});

/// The "outdated?" sender as a SEAM (the FakeGuideAnswerer idea): the page
/// depends on "a function that flags a version", tests override it with a
/// recorder, and only this provider knows there is HTTP underneath.
final outdatedFlagSenderProvider =
    Provider<Future<void> Function(String guideVersionId)>((ref) {
  return (guideVersionId) async {
    // Kind 0 = Outdated; anonymous by design (5.6) - no token required.
    await ref.read(dioProvider).post<void>(
      '/api/guides/guide-feedback',
      data: {'guideVersionId': guideVersionId, 'kind': 0},
    );
  };
});
