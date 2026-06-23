namespace Clinic_Booking.DTOs.MessageDTO
{
    public class ConversationDto
    {
        public Guid OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string? OtherUserImage { get; set; }
        public string LastMessage { get; set; }
        public string? LastMessageImageName { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
