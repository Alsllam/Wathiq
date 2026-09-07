import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/locale_provider.dart';
import '../../l10n/gen/app_localizations.dart';
import 'guide_body.dart';
import 'guide_detail_providers.dart';

/// Deep-linkable detail: the slug arrives from the ROUTE (/guides/:slug), the
/// data from the family provider keyed by it. ConsumerStatefulWidget because
/// `_flagged` is state only this screen cares about.
class GuideDetailPage extends ConsumerStatefulWidget {
  const GuideDetailPage({required this.slug, super.key});

  final String slug;

  @override
  ConsumerState<GuideDetailPage> createState() => _GuideDetailPageState();
}

class _GuideDetailPageState extends ConsumerState<GuideDetailPage> {
  bool _flagged = false;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final detail = ref.watch(guideDetailProvider(widget.slug));

    return Scaffold(
      // go_router supplies the back button: this page was PUSHED, so the
      // AppBar infers a pop - no manual back wiring.
      appBar: AppBar(title: Text(l10n.navGuides)),
      body: detail.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, _) => Center(child: Text(l10n.loadError)),
        data: (guide) {
          final version = guide.version;
          final theme = Theme.of(context);
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Text(isAr ? guide.titleAr : guide.titleEn,
                  style: theme.textTheme.headlineSmall),
              const SizedBox(height: 4),
              // Vision R2 on mobile: freshness right under the title.
              Text(
                '${l10n.lastVerified} '
                '${DateFormat.yMMMd(isAr ? 'ar' : 'en').format(version.lastVerifiedAt)}',
                style: theme.textTheme.bodySmall
                    ?.copyWith(color: theme.colorScheme.primary),
              ),
              const SizedBox(height: 16),
              if (version.requiredDocuments != null ||
                  version.fees != null ||
                  version.location != null)
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(12),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Flow analysis in action: inside each `if` the field
                        // is promoted from String? to String - no `!` needed.
                        if (version.requiredDocuments case final v?)
                          _Fact(label: l10n.requiredDocuments, value: v),
                        if (version.fees case final v?)
                          _Fact(label: l10n.fees, value: v),
                        if (version.location case final v?)
                          _Fact(label: l10n.location, value: v),
                      ],
                    ),
                  ),
                ),
              const SizedBox(height: 8),
              for (final segment in parseGuideBody(version.bodyMarkdown))
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: switch (segment) {
                    HeadingSegment() => Text(segment.text,
                        style: theme.textTheme.titleMedium),
                    BulletSegment() => Text('• ${segment.text}'),
                    ParagraphSegment() => Text(segment.text),
                  },
                ),
              if (version.steps.isNotEmpty) ...[
                Text(l10n.steps, style: theme.textTheme.titleMedium),
                const SizedBox(height: 8),
                for (final (index, step) in version.steps.indexed)
                  ListTile(
                    dense: true,
                    leading: CircleAvatar(
                      radius: 12,
                      child: Text('${index + 1}',
                          style: theme.textTheme.labelSmall),
                    ),
                    title: Text(step),
                  ),
              ],
              const Divider(height: 32),
              if (_flagged)
                Text(l10n.flagThanks,
                    style: TextStyle(color: theme.colorScheme.primary))
              else
                OutlinedButton(
                  onPressed: () async {
                    // read (not watch): a one-shot action, not a dependency.
                    await ref.read(outdatedFlagSenderProvider)(version.id);
                    if (mounted) setState(() => _flagged = true);
                  },
                  child: Text(l10n.flagOutdated),
                ),
            ],
          );
        },
      ),
    );
  }
}

class _Fact extends StatelessWidget {
  const _Fact({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label,
              style: theme.textTheme.labelSmall
                  ?.copyWith(color: theme.colorScheme.outline)),
          Text(value),
        ],
      ),
    );
  }
}
