using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.ClinicDTO
{
    public class ClinicUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
