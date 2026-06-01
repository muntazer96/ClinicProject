namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class DoctorSubscriptionAddDto
    {
        public int DoctorId { get; set; }
        public int PackageId { get; set; }
        public bool IsYearly { get; set; }
        public Clinic_Booking.Enums.SubscriptionStatus Status { get; set; } = Clinic_Booking.Enums.SubscriptionStatus.Active;
    }
}
