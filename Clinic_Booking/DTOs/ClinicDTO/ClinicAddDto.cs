using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.ClinicDTO
{
    public class ClinicAddDto
    {
        public string Name { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? ConsultationPrice { get; set; }
        public bool ShowConsultationPrice { get; set; } = false;
        public int? BookingWindowDays { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
