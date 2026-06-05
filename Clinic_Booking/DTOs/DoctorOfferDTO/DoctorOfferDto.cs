using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorOfferDTO
{
    public class DoctorOfferDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public bool AppliesToAllClinics { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DoctorOfferType OfferType { get; set; }
        public string OfferTypeName { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? OfferPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public string? BadgeText { get; set; }
        public string? Terms { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int DurationDays { get; set; }
        public int RemainingDays { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrentlyVisible { get; set; }
    }
}
