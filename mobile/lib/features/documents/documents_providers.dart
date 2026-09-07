import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import '../../core/models/document.dart';
import '../../core/models/list_result.dart';

/// The type catalogue is anonymous reference data (4.3's decision) - fetched
/// once, mapped by id so rows can show localized names without N lookups.
final documentTypesProvider =
    FutureProvider<Map<String, DocumentType>>((ref) async {
  final dio = ref.watch(dioProvider);
  final response =
      await dio.get<Map<String, dynamic>>('/api/documents/document-types');
  final types =
      ListResult.fromJson(response.data!, DocumentType.fromJson).items;
  return {for (final t in types) t.id: t};
});

/// The caller's documents (owner-scoped server-side; the bearer interceptor
/// supplies WHO). Sorted soonest-expiry-first by the API - same order as the
/// portal's list.
final documentsListProvider =
    FutureProvider<List<DocumentModel>>((ref) async {
  final dio = ref.watch(dioProvider);
  final response = await dio.get<Map<String, dynamic>>(
    '/api/documents/documents',
    queryParameters: {'MaxResultCount': 50},
  );
  return ListResult.fromJson(response.data!, DocumentModel.fromJson).items;
});

final documentDetailProvider =
    FutureProvider.family<DocumentModel, String>((ref, id) async {
  final dio = ref.watch(dioProvider);
  final response =
      await dio.get<Map<String, dynamic>>('/api/documents/documents/$id');
  return DocumentModel.fromJson(response.data!);
});
