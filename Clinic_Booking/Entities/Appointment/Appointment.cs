using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Appointment
{
    public class Appointment : BaseEntity<int>
    {
        public Guid UserId { get; set; } // معرف المستخدم الذي حجز
        public User.AspNetUsers User { get; set; }

        public int DoctorId { get; set; } // معرف الدكتور المحجوز
        public Doctor.Doctor Doctor { get; set; }

        public DateTime AppointmentDate { get; set; } // موعد الحجز
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // حالة الحجز (بانتظار، مؤكد، ملغي...)

        public decimal? PaymentAmount { get; set; } // المبلغ المدفوع (إن وجد)
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending; // حالة الدفع
        public string Code { get; set; }

    }
}
