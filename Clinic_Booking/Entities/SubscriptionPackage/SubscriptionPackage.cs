using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.SubscriptionPackage
{
    public class SubscriptionPackage : BaseEntity<int>
    {
        public string Name { get; set; } // اسم الباقة (مجاني، ذهبي، فضي ...)
        public decimal Price { get; set; } // سعر الاشتراك
        public int MaxDailyAppointments { get; set; } // أقصى عدد حجوزات يومية
        public int MaxWeeklyDays { get; set; } // أقصى عدد أيام استقبال للحجز بالأسبوع
        public bool ShowReviews { get; set; } // هل تظهر تقييمات الدكتور للمستخدمين؟
    }
}
