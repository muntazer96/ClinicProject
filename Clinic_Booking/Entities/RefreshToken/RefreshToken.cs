using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.RefreshToken
{
    public class RefreshToken : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public User.AspNetUsers User { get; set; } = null!;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsExpired => ExpiresAt <= BusinessClock.Now();
        public bool IsActive => !IsRevoked && !IsExpired && !IsDeleted;
    }
}
