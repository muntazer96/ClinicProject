namespace Clinic_Booking.DTOs.FeatureDTO
{
    public class GetFeatureDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string? Description { get; set; }
        public bool IsPremiumOnly { get; set; }
    }
}
