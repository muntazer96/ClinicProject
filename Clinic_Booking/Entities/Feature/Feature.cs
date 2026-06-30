using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Feature
{
    public class Feature : BaseEntity<int>
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string? Description { get; set; }
        public bool IsPremiumOnly { get; set; }
        public ICollection<DoctorFeature.DoctorFeature> DoctorFeatures { get; set; }
    }
}
