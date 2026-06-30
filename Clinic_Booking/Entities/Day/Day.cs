using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Day
{
    public class Day : BaseEntity<int>
    {
        public string Name { get; set; } // اسم اليوم
        public string NormalizedName { get; set; }
    }
}
