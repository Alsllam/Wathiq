/// Documents models (UC-01's read side, consumed from 6.7).
library;

/// Wire values of Wathiq.Documents.DocumentStatus (tinyint on the API).
/// An enhanced enum maps int ↔ value in one place instead of scattered ==.
enum DocumentStatus {
  active(0),
  archived(1);

  const DocumentStatus(this.wire);
  final int wire;

  static DocumentStatus fromWire(int value) =>
      values.firstWhere((s) => s.wire == value);
}

class AttachmentModel {
  const AttachmentModel({
    required this.id,
    required this.mimeType,
    required this.sizeBytes,
  });

  final String id;
  final String mimeType;
  final int sizeBytes;

  factory AttachmentModel.fromJson(Map<String, dynamic> json) =>
      AttachmentModel(
        id: json['id'] as String,
        mimeType: json['mimeType'] as String,
        sizeBytes: json['sizeBytes'] as int,
      );
}

class DocumentModel {
  const DocumentModel({
    required this.id,
    required this.holderId,
    required this.documentTypeId,
    required this.status,
    this.number,
    this.issueDate,
    this.expiryDate,
    this.notes,
    this.daysUntilExpiry,
    this.attachments = const [],
  });

  final String id;
  final String holderId;
  final String documentTypeId;
  final DocumentStatus status;

  // All optional on the wire (FR-DOC-002: dates and number are never forced) -
  // so all `?` here. The compiler now FORCES every screen to design its
  // "no expiry date" state instead of discovering it in production.
  final String? number;
  final DateTime? issueDate;
  final DateTime? expiryDate;
  final String? notes;
  final int? daysUntilExpiry;
  final List<AttachmentModel> attachments;

  factory DocumentModel.fromJson(Map<String, dynamic> json) => DocumentModel(
        id: json['id'] as String,
        holderId: json['holderId'] as String,
        documentTypeId: json['documentTypeId'] as String,
        status: DocumentStatus.fromWire(json['status'] as int),
        number: json['number'] as String?,
        issueDate: _dateOrNull(json['issueDate']),
        expiryDate: _dateOrNull(json['expiryDate']),
        notes: json['notes'] as String?,
        daysUntilExpiry: json['daysUntilExpiry'] as int?,
        attachments: (json['attachments'] as List<dynamic>? ?? const [])
            .map((e) => AttachmentModel.fromJson(e as Map<String, dynamic>))
            .toList(),
      );

  static DateTime? _dateOrNull(dynamic value) =>
      value == null ? null : DateTime.parse(value as String);
}

class DocumentType {
  const DocumentType({
    required this.id,
    required this.code,
    required this.nameAr,
    required this.nameEn,
    this.defaultValidityMonths,
  });

  final String id;
  final String code;
  final String nameAr;
  final String nameEn;
  final int? defaultValidityMonths;

  factory DocumentType.fromJson(Map<String, dynamic> json) => DocumentType(
        id: json['id'] as String,
        code: json['code'] as String,
        nameAr: json['nameAr'] as String,
        nameEn: json['nameEn'] as String,
        defaultValidityMonths: json['defaultValidityMonths'] as int?,
      );
}
