import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';

/// Our own source enum - plugin types never leak past this file (the wall).
enum CaptureSource { camera, gallery }

/// Bytes + metadata, not a path: the upload needs bytes anyway, and bytes are
/// trivially fakeable in tests where no filesystem/camera exists.
class CapturedImage {
  const CapturedImage({
    required this.bytes,
    required this.fileName,
    required this.mimeType,
  });

  final Uint8List bytes;
  final String fileName;
  final String mimeType;
}

/// The 6.6 wall pattern for the camera: consumers say "give me a photo",
/// only this impl knows the OS is involved. Null = user cancelled the picker,
/// which is a normal outcome, not an error.
abstract class ImageCapture {
  Future<CapturedImage?> pick(CaptureSource source);
}

class ImagePickerCapture implements ImageCapture {
  ImagePickerCapture();

  final ImagePicker _picker = ImagePicker();

  @override
  Future<CapturedImage?> pick(CaptureSource source) async {
    // The OS mediates: an intent/system sheet asks THE USER, not us - the app
    // never touches the camera roll, it receives exactly one blessed file.
    // (iOS still requires the Info.plist usage strings, or this call crashes.)
    final file = await _picker.pickImage(
      source: source == CaptureSource.camera
          ? ImageSource.camera
          : ImageSource.gallery,
      maxWidth: 2400, // capped: OCR needs legibility, not a 12MP upload (3.5)
      imageQuality: 85,
    );
    if (file == null) {
      return null;
    }
    return CapturedImage(
      bytes: await file.readAsBytes(),
      fileName: file.name,
      mimeType: file.mimeType ?? 'image/jpeg',
    );
  }
}

final imageCaptureProvider =
    Provider<ImageCapture>((ref) => ImagePickerCapture());
