namespace Clinic_Booking.Configuration
{
    public class AppointmentReminderOptions
    {
        public const string SectionName = "AppointmentReminders";

        public bool Enabled { get; set; } = true;
        public int RunHour { get; set; } = 7;
        public int RunMinute { get; set; } = 0;
        public int RepeatEveryHoursWhenMissed { get; set; } = 1;
    }
}
