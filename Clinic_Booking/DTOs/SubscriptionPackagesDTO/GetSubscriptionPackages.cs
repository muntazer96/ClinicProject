namespace Clinic_Booking.DTOs.SubscriptionPackagesDTO
{
    public class GetSubscriptionPackages
    {
        public int Id { get; set; }
        public string Name { get; set; } // اسم الباقة (مجاني، ذهبي، فضي ...)
        public string NormalizedName { get; set; } // اسم الباقة (مجاني، ذهبي، فضي ...)
        public decimal Price { get; set; } // سعر الاشتراك
        public decimal YearlyPrice { get; set; }
        public int MaxClinics { get; set; } // أقصى عدد عيادات للطبيب
        public int MaxDailyAppointments { get; set; } // أقصى عدد حجوزات يومية
        public int MaxWeeklyDays { get; set; } // أقصى عدد أيام استقبال للحجز بالأسبوع
        public bool ShowReviews { get; set; } // هل تظهر تقييمات الدكتور للمستخدمين؟
        public bool ShowMessages { get; set; }
        public bool EBooking { get; set; }
        public bool EPayments { get; set; }
        public bool MakeOffers { get; set; }
        public int MaxActiveOffers { get; set; }
    }
}
