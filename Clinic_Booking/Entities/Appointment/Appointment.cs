using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;
using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Entities.Appointment
{
    public class Appointment : BaseEntity<int>
    {
        public Guid? UserId { get; set; }
        public User.AspNetUsers? User { get; set; }

        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }
        public int ClinicId { get; set; }
        public Clinic.Clinic Clinic { get; set; }

        public DateTime AppointmentDate { get; set; }
        public int QueueNumber { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
        public string? Notes { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public string? CancellationReason { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public DateTime? CancelledAt { get; set; }

        public decimal? PaymentAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        [StringLength(50)]
        public string Code { get; set; }
        public ICollection<Review.Review> Reviews { get; set; } = [];

    }
}
