using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.ReviewDTO
{
    public class AddReviewDto
    {
        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }
        [Range(1, int.MaxValue)]
        public int AppointmentId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }
    }
}
