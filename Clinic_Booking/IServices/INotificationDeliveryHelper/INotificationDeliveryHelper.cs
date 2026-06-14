using Clinic_Booking.Enums;

namespace Clinic_Booking.IServices.INotificationDeliveryHelper
{
    public sealed record PendingAppointmentNotification(
        int DoctorId,
        int ClinicId,
        int AppointmentId,
        Guid? UserId,
        string? GuestPhoneNumber,
        AppointmentStatus Status,
        string Title,
        string Body);

    public interface INotificationDeliveryHelper
    {
        Task SendPendingNotificationsAsync(List<PendingAppointmentNotification> notifications);
    }
}
