namespace Clinic_Booking.DTOs.AnalyticsDTO
{
    public class AnalyticsSummaryDto
    {
        public List<MetricDto> Metrics { get; set; } = new();
        public List<LabelValueDto> AppointmentStatus { get; set; } = new();
        public List<LabelValueDto> AppointmentSources { get; set; } = new();
        public List<LabelValueDto> TopDoctorsByViews { get; set; } = new();
        public List<LabelValueDto> TopDoctorsByBookings { get; set; } = new();
        public List<LabelValueDto> TopSpecializationsBySearch { get; set; } = new();
        public List<LabelValueDto> TopSpecializationsByBookings { get; set; } = new();
        public List<LabelValueDto> TopProvinces { get; set; } = new();
        public List<LabelValueDto> TopSearchTerms { get; set; } = new();
        public List<LabelValueDto> TopClinicsByBookings { get; set; } = new();
        public List<LabelValueDto> TopBookingDays { get; set; } = new();
        public List<LabelValueDto> PeakBookingHours { get; set; } = new();
        public List<LabelValueDto> TopPages { get; set; } = new();
        public List<AnalyticsEventItemDto> RecentEvents { get; set; } = new();
        public List<TrendPointDto> UserGrowth { get; set; } = new();
        public List<TrendPointDto> AppointmentTrend { get; set; } = new();
        public OfferAnalyticsDto Offers { get; set; } = new();
        public SubscriptionAnalyticsDto Subscriptions { get; set; } = new();
        public ConversionAnalyticsDto Conversions { get; set; } = new();
    }

    public class MetricDto
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Note { get; set; }
    }

    public class LabelValueDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class TrendPointDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }

    public class OfferAnalyticsDto
    {
        public int Views { get; set; }
        public int Clicks { get; set; }
        public int BookingsFromOffers { get; set; }
    }

    public class SubscriptionAnalyticsDto
    {
        public int ActiveSubscribers { get; set; }
        public int PremiumSubscribers { get; set; }
        public int BasicSubscribers { get; set; }
        public int ExpiredSubscriptions { get; set; }
        public int ExpiringSoon { get; set; }
    }

    public class ConversionAnalyticsDto
    {
        public decimal SearchToProfileRate { get; set; }
        public decimal ProfileToBookingRate { get; set; }
    }

    public class AnalyticsEventItemDto
    {
        public string EventType { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string? Source { get; set; }
        public string? Page { get; set; }
        public int? DoctorId { get; set; }
        public int? OfferId { get; set; }
    }
}
