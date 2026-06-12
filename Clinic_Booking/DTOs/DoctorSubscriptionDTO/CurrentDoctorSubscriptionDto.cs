namespace Clinic_Booking.DTOs.DoctorSubscriptionDTO
{
    public class CurrentDoctorSubscriptionDto
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string PackageArabicName { get; set; } = string.Empty;
        public string PackageEnglishName { get; set; } = string.Empty;
        public string PackageNormalizedName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal YearlyPrice { get; set; }
        public int MaxClinics { get; set; }
        public int MaxDailyAppointments { get; set; }
        public int MaxWeeklyDays { get; set; }
        public bool ShowReviews { get; set; }
        public bool ShowMessages { get; set; }
        public bool EBooking { get; set; }
        public bool EPayments { get; set; }
        public bool MakeOffers { get; set; }
        public int MaxActiveOffers { get; set; }
        public CurrentDoctorSubscriptionPackageDto Package { get; set; } = new();
        public List<CurrentDoctorSubscriptionFeatureDto> Features { get; set; } = [];
    }

    public class CurrentDoctorSubscriptionPackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ArabicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal YearlyPrice { get; set; }
        public int MaxClinics { get; set; }
        public int MaxDailyAppointments { get; set; }
        public int MaxWeeklyDays { get; set; }
        public bool ShowReviews { get; set; }
        public bool ShowMessages { get; set; }
        public bool EBooking { get; set; }
        public bool EPayments { get; set; }
        public bool MakeOffers { get; set; }
        public int MaxActiveOffers { get; set; }
    }

    public class CurrentDoctorSubscriptionFeatureDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
    }
}
