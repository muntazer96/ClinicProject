namespace Clinic_Booking.Configuration
{
    public class WhatsAppMessageOptions
    {
        public const string SectionName = "WhatsAppMessages";

        public bool Enabled { get; set; }
        public string Provider { get; set; } = "CustomHttp";
        public string Endpoint { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string PhoneFieldName { get; set; } = "phone";
        public string MessageFieldName { get; set; } = "message";
    }
}
