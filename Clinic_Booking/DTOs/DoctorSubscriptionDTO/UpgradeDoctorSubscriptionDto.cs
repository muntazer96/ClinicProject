using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class UpgradeDoctorSubscriptionDto
    {
        [Range(1, int.MaxValue)]
        public int PackageId { get; set; }
    }
}
