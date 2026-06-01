using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorDTO
{
    public class DoctorProfileUpdateDto
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int SpecializationId { get; set; }
        public string Description { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public IFormFile? ImageName { get; set; }
        public DateOnly BirthDay { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
    }
}
