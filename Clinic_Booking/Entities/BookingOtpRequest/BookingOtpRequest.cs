using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.BookingOtpRequest
{
    public class BookingOtpRequest : BaseEntity<int>
    {
        public int AppointmentId { get; set; }
        public Appointment.Appointment Appointment { get; set; }
        public string PhoneNumber { get; set; }
        public string CodeHash { get; set; }
        public string CodeSalt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime SentAt { get; set; }
        public int AttemptCount { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
