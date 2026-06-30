using Clinic_Booking.Enums;
using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class CreateDoctorRequestDto
    {
        [Required]
        public int VerificationTokenId { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        [Required]
        [StringLength(200)]
        public string KnownName { get; set; }

        [Required]
        public IraqiProvince Province { get; set; }

        [Required]
        public DateOnly BirthDay { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار تخصص صحيح.")]
        public int SpecializationId { get; set; }

        [Required]
        public IFormFile IdentityFront { get; set; }

        public IFormFile? IdentityBack { get; set; }
    }
}
