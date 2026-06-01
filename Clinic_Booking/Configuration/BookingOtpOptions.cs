namespace Clinic_Booking.Configuration
{
    public class BookingOtpOptions
    {
        public const string SectionName = "BookingOtp";

        public bool Enabled { get; set; }
        public int CodeLength { get; set; } = 6;
        public int ExpirationMinutes { get; set; } = 5;
        public int ResendCooldownSeconds { get; set; } = 60;
        public int MaxAttempts { get; set; } = 5;
    }
}
