using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class ResendBookingOtpDto
    {
        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        public string BookingCode { get; set; }
    }
}
