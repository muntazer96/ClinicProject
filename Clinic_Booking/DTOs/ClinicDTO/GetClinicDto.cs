using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.ClinicDTO
{
    public class GetClinicDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string Name { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string IraqiProvinceName { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal? ConsultationPrice { get; set; }
        public bool ShowConsultationPrice { get; set; }
        public bool IsVisible { get; set; }
    }
}
