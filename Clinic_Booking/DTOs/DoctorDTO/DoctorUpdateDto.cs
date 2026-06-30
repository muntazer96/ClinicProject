using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorDTO
{
    public class DoctorUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int SpecializationId { get; set; }
        public string Description { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public IFormFile? ImageName { get; set; } // Nullable
        public DateOnly BirthDay { get; set; }
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string PhoneNumber { get; set; }
        public string Location { get; set; }

    }
}
