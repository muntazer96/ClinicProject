using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.SubscriptionPackagesDTO;

namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class GetDoctorSubscriptionDto
    {
        public int Id { get; set; }
        public GetDoctorDto Doctor { get; set; }
        public GetSubscriptionPackages Package { get; set; }
        public DateTime StartDate { get; set; } // تاريخ بداية الاشتراك
        public DateTime EndDate { get; set; } // تاريخ نهاية الاشتراكpublic int DoctorId { get; set; }
        public bool IsActive { get; set; }
        public Clinic_Booking.Enums.SubscriptionStatus Status { get; set; }
        public DateTime? CancelledAt { get; set; }

    }
}
