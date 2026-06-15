using Microsoft.AspNetCore.Identity;

namespace Clinic_Booking.Entities.Role
{
    public class AspNetRoles : IdentityRole<Guid>
    {
        public int? DeleterId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int? CreatorId { get; set; }
        public DateTime? CreatedAt { get; set; } = BusinessClock.Now();
        public int? ModifierId { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
