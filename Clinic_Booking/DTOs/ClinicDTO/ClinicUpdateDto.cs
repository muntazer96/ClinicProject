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
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string? PhoneNumber { get; set; }
        public decimal? ConsultationPrice { get; set; }
        public bool ShowConsultationPrice { get; set; } = false;
        public int? BookingWindowDays { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
