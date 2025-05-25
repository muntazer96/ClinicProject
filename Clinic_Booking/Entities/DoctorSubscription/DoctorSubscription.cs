using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.DoctorSubscription
{
    public class DoctorSubscription : BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }

        public int PackageId { get; set; }
        public SubscriptionPackage.SubscriptionPackage Package { get; set; }

        public DateTime StartDate { get; set; } // تاريخ بداية الاشتراك
        public DateTime EndDate { get; set; } // تاريخ نهاية الاشتراك
    }
}
