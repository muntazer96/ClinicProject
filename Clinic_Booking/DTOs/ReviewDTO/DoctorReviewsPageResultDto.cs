namespace Clinic_Booking.DTOs.ReviewDTO
{
    public class DoctorReviewsPageResultDto
    {
        public int DoctorId { get; set; }
        public bool IsEnabled { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<GetReviewDto> Items { get; set; } = [];
    }
}
