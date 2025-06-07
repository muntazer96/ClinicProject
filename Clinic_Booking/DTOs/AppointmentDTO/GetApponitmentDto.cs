using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class GetApponitmentDto
    {
        public int Id { get; set; }
        public GetUserReivew User { get; set; } // معرف المستخدم الذي حجز
        public GetDoctorReview Doctor { get; set; } // معرف الدكتور المحجوز
        public DateTime AppointmentDate { get; set; } // موعد الحجز
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // حالة الحجز (بانتظار، مؤكد، ملغي...)

        public decimal? PaymentAmount { get; set; } // المبلغ المدفوع (إن وجد)
        public PaymentStatus PaymentStatus { get; set; }  // حالة الدفع
    }
}
