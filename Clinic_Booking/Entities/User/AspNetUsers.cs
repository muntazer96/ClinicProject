using Microsoft.AspNetCore.Identity;

namespace Clinic_Booking.Entities.User
{
    public class AspNetUsers : IdentityUser<Guid>
    {
        public int? DeleterId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int? CreatorId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public int? ModifierId { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
