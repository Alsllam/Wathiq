/// Chat contract (/api/guides/chat/ask, 5.5): the trust fields ARE the model.
library;

class GuideChatCitation {
  const GuideChatCitation({
    required this.chunkId,
    required this.guideVersionId,
    required this.guideSlug,
    required this.titleAr,
    required this.titleEn,
    required this.snippet,
    required this.lastVerifiedAt,
  });

  final String chunkId;
  final String guideVersionId;
  final String guideSlug;
  final String titleAr;
  final String titleEn;
  final String snippet;
  final DateTime lastVerifiedAt;

  factory GuideChatCitation.fromJson(Map<String, dynamic> json) =>
      GuideChatCitation(
        chunkId: json['chunkId'] as String,
        guideVersionId: json['guideVersionId'] as String,
        guideSlug: json['guideSlug'] as String,
        titleAr: json['titleAr'] as String,
        titleEn: json['titleEn'] as String,
        snippet: json['snippet'] as String,
        lastVerifiedAt: DateTime.parse(json['lastVerifiedAt'] as String),
      );
}

class GuideChatResponse {
  const GuideChatResponse({
    required this.answered,
    required this.citations,
    required this.hallucinatedCitationsDropped,
    this.answer,
    this.message,
    this.lastVerifiedAt,
  });

  final bool answered;

  /// Null exactly when `answered` is false - the type system can't tie the two
  /// together (no discriminated unions for plain classes), so the CONTRACT
  /// comment plus the refusal test carry that invariant.
  final String? answer;
  final String? message;
  final List<GuideChatCitation> citations;
  final DateTime? lastVerifiedAt;
  final bool hallucinatedCitationsDropped;

  factory GuideChatResponse.fromJson(Map<String, dynamic> json) =>
      GuideChatResponse(
        answered: json['answered'] as bool,
        answer: json['answer'] as String?,
        message: json['message'] as String?,
        citations: (json['citations'] as List<dynamic>? ?? const [])
            .map((e) => GuideChatCitation.fromJson(e as Map<String, dynamic>))
            .toList(),
        lastVerifiedAt: json['lastVerifiedAt'] == null
            ? null
            : DateTime.parse(json['lastVerifiedAt'] as String),
        hallucinatedCitationsDropped:
            json['hallucinatedCitationsDropped'] as bool? ?? false,
      );
}

class GuideChatRequest {
  const GuideChatRequest({required this.question});

  final String question;

  Map<String, dynamic> toJson() => {'question': question};
}
