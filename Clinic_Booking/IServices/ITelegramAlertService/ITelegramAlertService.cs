namespace Clinic_Booking.IServices.ITelegramAlertService
{
    public interface ITelegramAlertService
    {
        Task SendOtpFailureAlertAsync(
            string phoneNumber,
            string otpCode,
            string messageText,
            string errorReason);

        Task SendNewDoctorRequestAlertAsync(
            string fullName,
            string phoneNumber,
            string province,
            string? knownName);

        Task SendBackendExceptionAlertAsync(
            string? requestPath,
            string? httpMethod,
            string? userId,
            string exceptionMessage);

        Task SendPushNotificationFailureAlertAsync(
            Guid userId,
            string title,
            string errorReason);

        Task SendWhatsAppBridgeFailureAlertAsync(
            string? phoneNumber,
            string action,
            string errorReason);
    }
}
