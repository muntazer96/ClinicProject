using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Clinic_Booking.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageType
    {
        [Display(Name = "رسالة عامة")]
        General,

        [Display(Name = "استفسار")]
        Inquiry,

        [Display(Name = "شكوى")]
        Complaint,

        [Display(Name = "متابعة")]
        FollowUp
    }
}
