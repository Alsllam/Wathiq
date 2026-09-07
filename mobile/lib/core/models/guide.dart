/// Guide models for the public reading surface (/api/guides/guide*).
/// Field-for-field the portal's aliases over the same wire (5.7's swagger).
library;

/// One list row. Every field is non-nullable because the API always sends
/// them - sound null safety means that promise is CHECKED: if `slug` were
/// missing, the `as String` cast throws at decode time, and nothing downstream
/// ever needs a null check. Nullability is reserved for fields that can
/// actually be absent.
class GuideSummary {
  const GuideSummary({
    required this.id,
    required this.slug,
    required this.titleAr,
    required this.titleEn,
  });

  // `required` named parameters: construction sites read like the JSON they
  // mirror, and forgetting a field is a compile error - C# object initializers
  // with the safety of constructors.
  final String id;
  final String slug;
  final String titleAr;
  final String titleEn;

  factory GuideSummary.fromJson(Map<String, dynamic> json) => GuideSummary(
        id: json['id'] as String,
        slug: json['slug'] as String,
        titleAr: json['titleAr'] as String,
        titleEn: json['titleEn'] as String,
      );
}

/// The served content of one language (immutable published version, 5.2).
class GuideVersion {
  const GuideVersion({
    required this.id,
    required this.versionNo,
    required this.language,
    required this.bodyMarkdown,
    required this.lastVerifiedAt,
    required this.steps,
    this.requiredDocuments,
    this.fees,
    this.location,
  });

  final String id;
  final int versionNo;
  final String language;
  final String bodyMarkdown;

  /// Vision R2 rides every read; the wire sends `"2026-09-01"` (a date-only
  /// string) and DateTime.parse handles it - time-of-day stays midnight.
  final DateTime lastVerifiedAt;
  final List<String> steps;

  // `String?` is a DIFFERENT TYPE from String, not an annotation: the compiler
  // refuses `fees.length` until a null check narrows it (flow analysis, like
  // TS strictNullChecks - unlike C# NRT there is no warning-only mode).
  final String? requiredDocuments;
  final String? fees;
  final String? location;

  factory GuideVersion.fromJson(Map<String, dynamic> json) => GuideVersion(
        id: json['id'] as String,
        versionNo: json['versionNo'] as int,
        language: json['language'] as String,
        bodyMarkdown: json['bodyMarkdown'] as String,
        lastVerifiedAt: DateTime.parse(json['lastVerifiedAt'] as String),
        steps: (json['steps'] as List<dynamic>? ?? const [])
            .map((e) => e as String)
            .toList(),
        // `as String?` accepts the value OR null/absent - the nullable cast is
        // how "optional on the wire" is spelled once, at the boundary.
        requiredDocuments: json['requiredDocuments'] as String?,
        fees: json['fees'] as String?,
        location: json['location'] as String?,
      );
}

class GuideDetail {
  const GuideDetail({
    required this.id,
    required this.slug,
    required this.titleAr,
    required this.titleEn,
    required this.version,
  });

  final String id;
  final String slug;
  final String titleAr;
  final String titleEn;
  final GuideVersion version;

  factory GuideDetail.fromJson(Map<String, dynamic> json) => GuideDetail(
        id: json['id'] as String,
        slug: json['slug'] as String,
        titleAr: json['titleAr'] as String,
        titleEn: json['titleEn'] as String,
        version:
            GuideVersion.fromJson(json['version'] as Map<String, dynamic>),
      );
}
