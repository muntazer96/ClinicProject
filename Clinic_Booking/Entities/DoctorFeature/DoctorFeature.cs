using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.DoctorFeature
{
    public class DoctorFeature : BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }
        public int FeatureId { get; set; }
        public Feature.Feature Feature { get; set; }
        public bool IsEnabled { get; set; }
    }
}
