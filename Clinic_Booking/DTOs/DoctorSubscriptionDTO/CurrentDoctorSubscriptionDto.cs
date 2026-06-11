namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class CurrentDoctorSubscriptionDto
    {
        public string PackageName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CurrentDoctorSubscriptionFeatureDto> Features { get; set; } = [];
    }

    public class CurrentDoctorSubscriptionFeatureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
    }
}
