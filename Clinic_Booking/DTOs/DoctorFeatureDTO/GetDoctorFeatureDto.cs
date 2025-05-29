using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.FeatureDTO;

namespace Clinic_Booking.DTOs.DoctorFeatureDTO
{
    public class GetDoctorFeatureDto
    {
        public int Id { get; set; }
        public GetDoctorDto Doctor { get; set; }
        public GetFeatureDto Feature { get; set; }
        public bool IsEnabled { get; set; }
    }
}
