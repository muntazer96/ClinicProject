namespace Clinic_Booking.Utilities
{
    public static class BusinessClock
    {
        private static readonly TimeZoneInfo BaghdadTimeZone = ResolveBaghdadTimeZone();

        public static DateTime Now() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BaghdadTimeZone);

        public static DateTimeOffset NowOffset() => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, BaghdadTimeZone);

        public static DateTime Today() => Now().Date;

        public static DateOnly TodayDateOnly() => DateOnly.FromDateTime(Today());

        private static TimeZoneInfo ResolveBaghdadTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time");
            }
        }
    }
}
