/// Reminder models (consumed from 6.9): system-managed rows, read-only here.
library;

enum ReminderStatus {
  pending(0),
  sent(1),
  failed(2),
  cancelled(3);

  const ReminderStatus(this.wire);
  final int wire;

  static ReminderStatus fromWire(int value) =>
      values.firstWhere((s) => s.wire == value);
}

class Reminder {
  const Reminder({
    required this.id,
    required this.documentId,
    required this.offsetDays,
    required this.dueDate,
    required this.expiryDate,
    required this.status,
    this.sentAt,
  });

  final String id;
  final String documentId;
  final int offsetDays;
  final DateTime dueDate;
  final DateTime expiryDate;
  final ReminderStatus status;
  final DateTime? sentAt;   // null until dispatched - history is immutable (2.x)

  factory Reminder.fromJson(Map<String, dynamic> json) => Reminder(
        id: json['id'] as String,
        documentId: json['documentId'] as String,
        offsetDays: json['offsetDays'] as int,
        dueDate: DateTime.parse(json['dueDate'] as String),
        expiryDate: DateTime.parse(json['expiryDate'] as String),
        status: ReminderStatus.fromWire(json['status'] as int),
        sentAt: json['sentAt'] == null
            ? null
            : DateTime.parse(json['sentAt'] as String),
      );
}
