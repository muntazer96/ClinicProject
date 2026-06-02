using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Appointment
{
    public class Appointment : BaseEntity<int>
    {
        public Guid? UserId { get; set; } // فارغ عندما يكون الحجز لزائر
        public User.AspNetUsers? User { get; set; }

        public int DoctorId { get; set; } // معرف الدكتور المحجوز
        public Doctor.Doctor Doctor { get; set; }
        public int ClinicId { get; set; } // فرع العيادة الذي تم الحجز فيه
        public Clinic.Clinic Clinic { get; set; }

        public DateTime AppointmentDate { get; set; } // تاريخ الدور، بدون توقيت ملزم للمراجع
        public int QueueNumber { get; set; } // رقم الدور في العيادة لهذا اليوم
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // حالة الحجز (بانتظار، مؤكد، ملغي...)
        public string? GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
        public string? Notes { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public string? CancellationReason { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public DateTime? CancelledAt { get; set; }

        public decimal? PaymentAmount { get; set; } // المبلغ المدفوع (إن وجد)
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending; // حالة الدفع
        public string Code { get; set; }
        public ICollection<Review.Review> Reviews { get; set; } = [];

    }
}
