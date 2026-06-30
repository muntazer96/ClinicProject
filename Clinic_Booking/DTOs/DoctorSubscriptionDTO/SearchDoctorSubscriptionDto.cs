namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class SearchDoctorSubscriptionDto
    {
        public int? Id { get; set; }
        public int? DoctorId { get; set; }
        public int? PackageId { get; set; }
        public bool? IsActive { get; set; }
        public Clinic_Booking.Enums.SubscriptionStatus? Status { get; set; }

    }
}
