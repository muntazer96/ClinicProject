using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class DoctorSubscriptionAddDto
    {
        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }
        [Range(1, int.MaxValue)]
        public int PackageId { get; set; }
        public bool IsYearly { get; set; }
        public Clinic_Booking.Enums.SubscriptionStatus Status { get; set; } = Clinic_Booking.Enums.SubscriptionStatus.Active;
    }
}
