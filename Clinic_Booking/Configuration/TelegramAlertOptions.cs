namespace Clinic_Booking.Configuration
{
    public class TelegramAlertOptions
    {
        public const string SectionName = "TelegramAlert";

        public bool Enabled { get; set; }
        public string BotToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = "Clinic Booking";
        public string EnvironmentName { get; set; } = "Production";
    }
}
