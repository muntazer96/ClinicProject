import '../../core/api_client.dart';
import 'models/booking_models.dart';

class BookingService {
  BookingService(this._client);

  final ApiClient _client;

  Future<List<QueueAvailability>> getAvailability(int clinicId) async {
    final response = await _client.dio.get(
      '/Appointment/queue-availability/$clinicId',
      queryParameters: {'days': 14},
    );
    final data = response.data['data'] as List? ?? const [];
    return data
        .map((item) => QueueAvailability.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<BookingResult> createBooking({
    required int doctorId,
    required int clinicId,
    required DateTime date,
    String? guestName,
    String? guestPhoneNumber,
    String? notes,
  }) async {
    final response = await _client.dio.post(
      '/Appointment',
      data: {
        'doctorId': doctorId,
        'clinicId': clinicId,
        'appointmentDate': _dateOnly(date),
        if (guestName?.trim().isNotEmpty == true)
          'guestName': guestName!.trim(),
        if (guestPhoneNumber?.trim().isNotEmpty == true)
          'guestPhoneNumber': guestPhoneNumber!.trim(),
        if (notes?.trim().isNotEmpty == true) 'notes': notes!.trim(),
      },
    );
    return BookingResult.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  Future<void> confirmOtp({
    required String phoneNumber,
    required String bookingCode,
    required String otpCode,
  }) async {
    await _client.dio.post(
      '/Appointment/otp/confirm',
      data: {
        'phoneNumber': phoneNumber.trim(),
        'bookingCode': bookingCode,
        'otpCode': otpCode.trim(),
      },
    );
  }

  Future<void> resendOtp({
    required String phoneNumber,
    required String bookingCode,
  }) async {
    await _client.dio.post(
      '/Appointment/otp/resend',
      data: {'phoneNumber': phoneNumber.trim(), 'bookingCode': bookingCode},
    );
  }

  Future<List<BookingDetails>> getMyBookings() async {
    final response = await _client.dio.get('/Appointment/my');
    final data = response.data['data'] as List? ?? const [];
    return data
        .map((item) => BookingDetails.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<BookingDetails> getGuestBooking({
    required String phoneNumber,
    required String bookingCode,
  }) async {
    final response = await _client.dio.get(
      '/Appointment/guest',
      queryParameters: {
        'phoneNumber': phoneNumber.trim(),
        'code': bookingCode.trim(),
      },
    );
    return BookingDetails.fromJson(
      response.data['data'] as Map<String, dynamic>,
    );
  }

  Future<void> cancelMyBooking(int appointmentId) async {
    await _client.dio.post(
      '/Appointment/my/cancel',
      data: {'appointmentId': appointmentId},
    );
  }

  Future<void> cancelGuestBooking({
    required String phoneNumber,
    required String bookingCode,
  }) async {
    await _client.dio.post(
      '/Appointment/guest/cancel',
      data: {'phoneNumber': phoneNumber.trim(), 'code': bookingCode.trim()},
    );
  }

  static String _dateOnly(DateTime date) =>
      '${date.year.toString().padLeft(4, '0')}-'
      '${date.month.toString().padLeft(2, '0')}-'
      '${date.day.toString().padLeft(2, '0')}';
}
