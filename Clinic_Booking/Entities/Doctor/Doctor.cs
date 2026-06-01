using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Doctor
{
    public class Doctor : BaseEntity<int>
    {
        public string Name { get; set; } // اسم الدكتور
        public string NormalizedName { get; set; }
        public int SpecializationId { get; set; } // معرف التخصص
        public Specialization.Specialization Specialization { get; set; } // علاقة التخصص
        public string Description { get; set; } // وصف الدكتور أو خبرته

        // عدد مرات الاشتراك في النظام (حسب الاشتراك)
        public int SubscriptionRank { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string ImageName { get; set; }
        public DateOnly BirthDay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public Guid? UserId { get; set; }
        public User.AspNetUsers? User { get; set; }
        public bool IsPubliclyVisible { get; set; }

        // العيادات التابعة للطبيب، ولكل عيادة عنوان وجدول مستقل
        public ICollection<Clinic.Clinic> Clinics { get; set; }

        // مراجعات المرضى عن الدكتور
        public ICollection<Review.Review> Reviews { get; set; }

        // اشتراكات الدكتور
        public ICollection<DoctorSubscription.DoctorSubscription> DoctorSubscriptions { get; set; }

        // الرسائل المرسلة إليه
        public ICollection<Message.Message> ReceivedMessages { get; set; }

        // الإشعارات المرسلة إليه
        public ICollection<Notification.Notification> Notifications { get; set; }
        public ICollection<DoctorFeature.DoctorFeature> DoctorFeatures { get; set; }
    }
}
