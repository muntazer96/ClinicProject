using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorDTO
{
    public class SearchDoctorDto
    {
        public int? Specialization { get; set; }
        public int? Id { get; set; }
        public string? Name { get; set; }
        public IraqiProvince? IraqiProvince { get; set; }
        public string? Sort { get; set; }
    }
}
