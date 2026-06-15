using Microsoft.AspNetCore.Identity;

namespace Clinic_Booking.Entities.User
{
    public class AspNetUsers : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? ImageName { get; set; }
        public bool IsLocked { get; set; } = false;
        public bool IsFirstLogin { get; set; } = true;
        public DateTime LastLoginDate { get; set; }
        public int? DeleterId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int? CreatorId { get; set; }
        public DateTime? CreatedAt { get; set; } = BusinessClock.Now();
        public int? ModifierId { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
