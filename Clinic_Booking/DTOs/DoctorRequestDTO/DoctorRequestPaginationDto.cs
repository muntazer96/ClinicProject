namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class DoctorRequestPaginationDto
    {
        public List<DoctorRequestListItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
