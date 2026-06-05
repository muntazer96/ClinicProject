namespace Clinic_Booking.Configuration
{
    public class PushNotificationOptions
    {
        public const string SectionName = "PushNotifications";

        public bool Enabled { get; set; }
        public string Provider { get; set; } = "Firebase";
        public string FcmEndpoint { get; set; } = "https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";
        public string? ServiceAccountPath { get; set; }
        public string? ProjectId { get; set; }
        public string? ServerKey { get; set; }
    }
}
