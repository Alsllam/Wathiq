import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart' hide TextDirection;

import '../../core/locale_provider.dart';
import '../../l10n/gen/app_localizations.dart';
import 'create_document.dart';
import 'documents_providers.dart';
import 'image_capture.dart';

/// THE screen that earns a State object (the 6.7 decision table): a text
/// controller, a picked date, captured bytes, a pending flag - all ephemeral,
/// pre-save, and nobody else's business. The moment Save succeeds, the truth
/// lives on the server and this state dies with the screen.
class AddDocumentPage extends ConsumerStatefulWidget {
  const AddDocumentPage({super.key});

  @override
  ConsumerState<AddDocumentPage> createState() => _AddDocumentPageState();
}

class _AddDocumentPageState extends ConsumerState<AddDocumentPage> {
  final _number = TextEditingController();
  String? _typeId;
  DateTime? _expiry;
  CapturedImage? _image;
  bool _saving = false;
  bool _failed = false;

  @override
  void dispose() {
    _number.dispose(); // controllers hold platform resources - State's duty
    super.dispose();
  }

  Future<void> _pick(CaptureSource source) async {
    final image = await ref.read(imageCaptureProvider).pick(source);
    if (image != null && mounted) {
      setState(() => _image = image);
    }
  }

  Future<void> _save() async {
    final typeId = _typeId;
    if (typeId == null || _saving) {
      return;
    }
    setState(() {
      _saving = true;
      _failed = false;
    });
    try {
      final id = await ref.read(createDocumentSenderProvider)(NewDocumentDraft(
        documentTypeId: typeId,
        number: _number.text.trim().isEmpty ? null : _number.text.trim(),
        expiryDate: _expiry,
        image: _image,
      ));
      if (mounted) {
        context.pushReplacement('/documents/$id'); // form gone, detail in its place
      }
    } on Exception {
      if (mounted) {
        setState(() {
          _saving = false;
          _failed = true; // stay on the form: nothing was lost, retry is free
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final types = ref.watch(documentTypesProvider).value ?? const {};

    return Scaffold(
      appBar: AppBar(title: Text(l10n.addDocument)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text(l10n.typeLabel, style: Theme.of(context).textTheme.titleSmall),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              for (final type in types.values)
                ChoiceChip(
                  label: Text(isAr ? type.nameAr : type.nameEn),
                  selected: _typeId == type.id,
                  onSelected: (_) => setState(() => _typeId = type.id),
                ),
            ],
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _number,
            textDirection: TextDirection.ltr, // document numbers are Latin
            decoration: InputDecoration(
              labelText: l10n.numberLabel,
              border: const OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: 16),
          OutlinedButton.icon(
            icon: const Icon(Icons.event_outlined),
            label: Text(_expiry == null
                ? l10n.pickExpiry
                : DateFormat.yMMMd(isAr ? 'ar' : 'en').format(_expiry!)),
            onPressed: () async {
              final now = DateTime.now();
              final picked = await showDatePicker(
                context: context,
                initialDate: now,
                firstDate: now.subtract(const Duration(days: 365)),
                lastDate: now.add(const Duration(days: 365 * 15)),
              );
              if (picked != null) {
                setState(() => _expiry = picked);
              }
            },
          ),
          const SizedBox(height: 16),
          if (_image case final image?)
            Card(
              child: ListTile(
                leading: const Icon(Icons.check_circle_outline),
                title: Text(l10n.photoAttached),
                subtitle: Text(
                    '${image.fileName} · ${(image.bytes.length / 1024).toStringAsFixed(0)} KB'),
                trailing: IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => setState(() => _image = null),
                ),
              ),
            )
          else
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    icon: const Icon(Icons.photo_camera_outlined),
                    label: Text(l10n.takePhoto),
                    onPressed: () => _pick(CaptureSource.camera),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: OutlinedButton.icon(
                    icon: const Icon(Icons.photo_library_outlined),
                    label: Text(l10n.fromGallery),
                    onPressed: () => _pick(CaptureSource.gallery),
                  ),
                ),
              ],
            ),
          const SizedBox(height: 24),
          if (_failed)
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Text(l10n.saveError,
                  style: TextStyle(color: Theme.of(context).colorScheme.error)),
            ),
          FilledButton(
            // Disabled until a type is chosen - the ONE required field (FR-DOC-002).
            onPressed: _typeId == null || _saving ? null : _save,
            child: _saving
                ? const SizedBox(
                    height: 20,
                    width: 20,
                    child: CircularProgressIndicator(strokeWidth: 2))
                : Text(l10n.save),
          ),
        ],
      ),
    );
  }
}
