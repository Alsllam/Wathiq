import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/api/api_client.dart';
import '../../core/models/list_result.dart';
import 'documents_providers.dart';
import 'image_capture.dart';

/// Everything the form collects; server fills the rest (status, reminders).
class NewDocumentDraft {
  const NewDocumentDraft({
    required this.documentTypeId,
    this.number,
    this.expiryDate,
    this.image,
  });

  final String documentTypeId;
  final String? number;
  final DateTime? expiryDate;
  final CapturedImage? image;
}

/// The submit seam (the outdatedFlagSender pattern, heavier duty): create the
/// document under the SELF holder, upload the photo if one was captured,
/// invalidate the list so the tab refetches - one function the page calls and
/// tests fake. Returns the new document id for navigation.
final createDocumentSenderProvider =
    Provider<Future<String> Function(NewDocumentDraft)>((ref) {
  return (draft) async {
    final dio = ref.read(dioProvider);

    // The self holder is materialised server-side on first call (FR-DOC-007).
    final holders = await dio
        .get<Map<String, dynamic>>('/api/documents/holders')
        .then((r) => r.data!);
    final self = ListResult.fromJson(holders, (j) => j)
        .items
        .firstWhere((h) => h['isSelf'] == true);

    final created = await dio.post<Map<String, dynamic>>(
      '/api/documents/documents',
      data: {
        'holderId': self['id'],
        'documentTypeId': draft.documentTypeId,
        // ?value = null-aware element: the key vanishes when the value is null.
        'number': ?draft.number,
        // date-only: reminders math is day-granular (2.x)
        'expiryDate': ?draft.expiryDate?.toIso8601String().substring(0, 10),
      },
    );
    final id = created.data!['id'] as String;

    if (draft.image case final image?) {
      // Multipart field name 'file' - from the swagger contract, not guessed.
      await dio.post<void>(
        '/api/documents/documents/$id/upload-attachment',
        data: FormData.fromMap({
          'file': MultipartFile.fromBytes(
            image.bytes,
            filename: image.fileName,
            contentType: DioMediaType.parse(image.mimeType),
          ),
        }),
      );
    }

    ref.invalidate(documentsListProvider); // the list tab refetches next look
    return id;
  };
});
