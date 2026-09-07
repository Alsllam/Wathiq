import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../core/locale_provider.dart';
import '../../l10n/gen/app_localizations.dart';
import 'documents_providers.dart';

/// Also a plain ConsumerWidget: everything shown derives from two providers
/// (detail by id, the type map). Camera/upload arrive in 6.8 - THAT screen
/// will earn state (a picked file is ephemeral, pre-save, single-screen).
class DocumentDetailPage extends ConsumerWidget {
  const DocumentDetailPage({required this.id, super.key});

  final String id;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final types = ref.watch(documentTypesProvider).value ?? const {};

    return Scaffold(
      appBar: AppBar(title: Text(l10n.navDocuments)),
      body: ref.watch(documentDetailProvider(id)).when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (error, _) => Center(child: Text(l10n.loadError)),
            data: (doc) {
              final theme = Theme.of(context);
              final type = types[doc.documentTypeId];
              final dateFormat = DateFormat.yMMMd(isAr ? 'ar' : 'en');
              return ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  Text(
                    type == null ? (doc.number ?? '—') : (isAr ? type.nameAr : type.nameEn),
                    style: theme.textTheme.headlineSmall,
                  ),
                  if (doc.number case final number?)
                    Padding(
                      padding: const EdgeInsets.only(top: 4),
                      child: Text(number, style: theme.textTheme.bodyLarge),
                    ),
                  const SizedBox(height: 16),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          _Row(
                              label: l10n.issueDate,
                              value: doc.issueDate == null
                                  ? '—'
                                  : dateFormat.format(doc.issueDate!)),
                          _Row(
                              label: l10n.expiryDate,
                              value: doc.expiryDate == null
                                  ? '—'
                                  : dateFormat.format(doc.expiryDate!)),
                          if (doc.notes case final notes?)
                            _Row(label: l10n.notes, value: notes),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(l10n.attachments, style: theme.textTheme.titleMedium),
                  const SizedBox(height: 8),
                  if (doc.attachments.isEmpty)
                    Text(l10n.noAttachments,
                        style: TextStyle(color: theme.colorScheme.outline))
                  else
                    for (final attachment in doc.attachments)
                      ListTile(
                        dense: true,
                        leading: Icon(
                          attachment.mimeType.startsWith('image/')
                              ? Icons.image_outlined
                              : Icons.picture_as_pdf_outlined,
                        ),
                        title: Text(attachment.mimeType),
                        subtitle: Text(
                            '${(attachment.sizeBytes / 1024).toStringAsFixed(0)} KB'),
                      ),
                ],
              );
            },
          ),
    );
  }
}

class _Row extends StatelessWidget {
  const _Row({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 110,
            child: Text(label,
                style: theme.textTheme.labelMedium
                    ?.copyWith(color: theme.colorScheme.outline)),
          ),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }
}
