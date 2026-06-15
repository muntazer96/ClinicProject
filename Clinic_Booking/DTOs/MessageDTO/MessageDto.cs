using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.MessageDTO
{
    public class MessageDto
    {
        public int Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string? SenderImage { get; set; }
        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string? ReceiverImage { get; set; }
        public string Content { get; set; }
        public string? ImageName { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public MessageType Type { get; set; }
    }
}
