namespace Clinic_Booking.DTOs.DoctorOfferDTO
{
    public class SearchDoctorOfferDto
    {
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public bool? IsActive { get; set; }
        public bool? CurrentlyVisible { get; set; }
        public string? Search { get; set; }
    }
}
