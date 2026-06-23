using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Message
{
    public class Message : BaseEntity<int>
    {
        public Guid SenderId { get; set; }
        public AspNetUsers Sender { get; set; }

        public Guid ReceiverId { get; set; }
        public AspNetUsers Receiver { get; set; }

        public string Content { get; set; } = string.Empty;
        public string? ImageName { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public MessageType Type { get; set; }
    }
}
