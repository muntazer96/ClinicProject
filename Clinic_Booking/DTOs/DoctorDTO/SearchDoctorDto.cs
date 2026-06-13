using System.ComponentModel.DataAnnotations;
using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorDTO
{
    public class SearchDoctorDto
    {
        public int? Specialization { get; set; }
        public int? Id { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }
        public IraqiProvince? IraqiProvince { get; set; }

        [StringLength(50)]
        public string? Sort { get; set; }
    }
}
