using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Specialization
{
    public class Specialization : BaseEntity<int>
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string IconName { get; set; }
    }
}
