/// ABP's list envelope, decoded generically. Dart has no runtime type from
/// which to conjure `T.fromJson` (no reflection here), so the item decoder is
/// passed IN as a function - the same move as passing a deserializer lambda,
/// and the first taste of Dart's first-class function types.
class ListResult<T> {
  const ListResult(this.items);

  final List<T> items;

  factory ListResult.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromItem,
  ) {
    // strict-casts (analysis_options) forbids implicit dynamic → typed: every
    // step out of the JSON map is an EXPLICIT cast, so the wire's shape is
    // visible - and a surprise shape fails loudly at the cast, not three
    // screens later.
    final raw = json['items'] as List<dynamic>? ?? const [];
    return ListResult(
      raw.map((e) => fromItem(e as Map<String, dynamic>)).toList(),
    );
  }
}
