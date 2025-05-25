using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Enums
{
    public enum NotificationStatus
    {
        [Display(Name = "غير مقروءة")]
        Unread,

        [Display(Name = "مقروءة")]
        Read
    }
}
