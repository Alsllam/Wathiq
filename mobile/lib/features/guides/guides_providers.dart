import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import '../../core/models/guide.dart';
import '../../core/models/list_result.dart';

/// The httpResource analogue: a FutureProvider IS "a value derived from an
/// async source" - consumers get an AsyncValue (loading/error/data) and the
/// provider caches until invalidated or a dependency (the Dio client, hence
/// the locale) changes. Anonymous endpoint on purpose: 6.4 learns networking
/// with zero auth stacked on top (5.1's public-read design paying out again).
final guidesListProvider = FutureProvider<List<GuideSummary>>((ref) async {
  final dio = ref.watch(dioProvider);
  final response = await dio.get<Map<String, dynamic>>('/api/guides/guide');
  return ListResult.fromJson(response.data!, GuideSummary.fromJson).items;
});
