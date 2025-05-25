using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Payment
{
    public class Payment : BaseEntity<int>
    {
        public int AppointmentId { get; set; } // معرف الحجز المرتبط
        public Appointment.Appointment Appointment { get; set; }

        public decimal Amount { get; set; } // المبلغ المدفوع
        public DateTime PaidAt { get; set; } // وقت الدفع
        public PaymentStatus Status { get; set; } // حالة الدفع
    }
}
