using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.DeviceToken
{
    public class DeviceToken : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public User.AspNetUsers User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public DateTime LastSeenAt { get; set; } = BusinessClock.Now();
    }
}
