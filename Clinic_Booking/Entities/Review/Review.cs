using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Review
{
    public class Review : BaseEntity<int>
    {
        public Guid UserId { get; set; } // معرف المستخدم الذي قيم
        public User.AspNetUsers User { get; set; }

        public int DoctorId { get; set; } // معرف الدكتور الذي تم تقييمه
        public Doctor.Doctor Doctor { get; set; }

        public int Rating { get; set; } // التقييم من 1 إلى 5
        public string Comment { get; set; } // تعليق المستخدم
        public int? AppoinmentId { get; set; }
        public Appointment.Appointment? Appointment { get; set; }
    }
}
