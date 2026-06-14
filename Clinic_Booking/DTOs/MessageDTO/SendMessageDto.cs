using System.ComponentModel.DataAnnotations;
using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.MessageDTO
{
    public class SendMessageDto
    {
        [Required]
        public Guid ReceiverId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        public MessageType Type { get; set; } = MessageType.General;
    }
}
