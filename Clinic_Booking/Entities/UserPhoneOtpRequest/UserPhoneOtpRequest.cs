using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Entities.User;

namespace Clinic_Booking.Entities.UserPhoneOtpRequest
{
    public class UserPhoneOtpRequest : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public AspNetUsers User { get; set; }
        public string PhoneNumber { get; set; }
        public string CodeHash { get; set; }
        public string CodeSalt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime SentAt { get; set; }
        public int AttemptCount { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
