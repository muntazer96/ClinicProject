using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.MessageDTO
{
    public class SendImageMessageDto
    {
        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        public IFormFile File { get; set; }

        [MaxLength(2000)]
        public string? Content { get; set; }
    }
}
