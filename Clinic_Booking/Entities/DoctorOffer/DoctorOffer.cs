using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.DoctorOffer
{
    public class DoctorOffer : BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }
        public int? ClinicId { get; set; }
        public Clinic.Clinic? Clinic { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DoctorOfferType OfferType { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? OfferPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public string? BadgeText { get; set; }
        public string? Terms { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
