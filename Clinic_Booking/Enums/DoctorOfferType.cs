using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.Enums
{
    public enum DoctorOfferType
    {
        [Display(Name = "خصم نسبة")]
        PercentageDiscount = 0,

        [Display(Name = "سعر خاص")]
        FixedPrice = 1,

        [Display(Name = "باقة خدمة")]
        ServicePackage = 2,

        [Display(Name = "استشارة مجانية")]
        FreeConsultation = 3
    }
}
