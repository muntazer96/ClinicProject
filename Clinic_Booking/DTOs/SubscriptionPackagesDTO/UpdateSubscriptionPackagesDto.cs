namespace Clinic_Booking.DTOs.SubscriptionPackagesDTO
{
    public class UpdateSubscriptionPackagesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
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
}
