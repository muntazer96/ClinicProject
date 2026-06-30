using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending,

        [Display(Name = "تم الدفع")]
        Completed,

        [Display(Name = "فشل الدفع")]
        Failed
    }
}
