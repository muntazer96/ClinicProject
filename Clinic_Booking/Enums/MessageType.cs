using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Enums
{
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
