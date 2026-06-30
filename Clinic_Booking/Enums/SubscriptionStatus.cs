using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Enums
{
    public enum SubscriptionStatus
    {
        [Display(Name = "نشط")]
        Active = 0,

        [Display(Name = "قيد الانتظار")]
        Pending = 1,

        [Display(Name = "منتهي")]
        Expired = 2,

        [Display(Name = "ملغي")]
        Cancelled = 3
    }
}
