using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Specialization
{
    public class Specialization : BaseEntity<int>
    {
        public int Id { get; set; } // رقم التخصص (المفتاح الأساسي)
        public string Name { get; set; } // اسم التخصص (مثل "طب الأطفال")
        public string NormalizedName { get; set; }
    }
}
