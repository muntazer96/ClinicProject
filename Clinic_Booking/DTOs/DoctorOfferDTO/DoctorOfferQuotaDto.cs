namespace Clinic_Booking.DTOs.DoctorOfferDTO
{
    public class DoctorOfferQuotaDto
    {
        public int DoctorId { get; set; }
        public bool CanMakeOffers { get; set; }
        public int MaxActiveOffers { get; set; }
        public int ActiveOffers { get; set; }
        public int RemainingOffers { get; set; }
        public string? PackageName { get; set; }
    }
}
