using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.UserFavoriteDoctor
{
    public class UserFavoriteDoctor : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public User.AspNetUsers User { get; set; }
        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }
    }
}
