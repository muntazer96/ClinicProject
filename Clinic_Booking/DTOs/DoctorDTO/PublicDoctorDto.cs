using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorDTO
{
    public class PublicDoctorListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public string SpecializationNormalizedName { get; set; }
        public string SpecializationIconName { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public bool CanBookOnline { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFeatured { get; set; }
        public string? ActiveSubscriptionName { get; set; }
        public string? ActiveSubscriptionNormalizedName { get; set; }
        public decimal ActiveSubscriptionWeight { get; set; }
        public List<PublicDoctorClinicSummaryDto> Clinics { get; set; } = [];
    }

    public class PublicDoctorProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public string SpecializationNormalizedName { get; set; }
        public string SpecializationIconName { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public bool CanBookOnline { get; set; }
        public bool CanMessage { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFeatured { get; set; }
        public string? ActiveSubscriptionName { get; set; }
        public string? ActiveSubscriptionNormalizedName { get; set; }
        public decimal ActiveSubscriptionWeight { get; set; }
        public Guid? UserId { get; set; }
        public List<PublicDoctorClinicDto> Clinics { get; set; } = [];
    }

    public class PublicDoctorClinicSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string IraqiProvinceName { get; set; }
        public string Address { get; set; }
        public decimal? ConsultationPrice { get; set; }
        public bool ShowConsultationPrice { get; set; }
        public int BookingWindowDays { get; set; }
    }

    public class PublicDoctorClinicDto : PublicDoctorClinicSummaryDto
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public List<PublicClinicAvailabilityDto> Availabilities { get; set; } = [];
    }

    public class PublicClinicAvailabilityDto
    {
        public int DayId { get; set; }
        public string DayName { get; set; }
        public string DayNormalizedName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxAppointments { get; set; }
    }
}
