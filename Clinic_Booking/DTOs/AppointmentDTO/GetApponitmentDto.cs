using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class GetApponitmentDto
    {
        public int Id { get; set; }
        public GetUserReivew? User { get; set; } // فارغ عندما يكون الحجز لزائر
        public GetDoctorReview Doctor { get; set; } // معرف الدكتور المحجوز
        public global::Clinic_Booking.DTOs.ClinicDTO.GetClinicDto Clinic { get; set; }
        public DateTime AppointmentDate { get; set; } // موعد الحجز
        public int QueueNumber { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
        public bool IsGuestBooking { get; set; }
        public string BookingSource { get; set; } = string.Empty;
        public bool IsPhoneConfirmed { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // حالة الحجز (بانتظار، مؤكد، ملغي...)

        public decimal? PaymentAmount { get; set; } // المبلغ المدفوع (إن وجد)
        public PaymentStatus PaymentStatus { get; set; }  // حالة الدفع
    }
}
