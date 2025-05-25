using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Enums
{
    public enum AppointmentStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending,

        [Display(Name = "مؤكد")]
        Confirmed,

        [Display(Name = "ملغي")]
        Cancelled,

        [Display(Name = "مكتمل")]
        Completed
    }
}
