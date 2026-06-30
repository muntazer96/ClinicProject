namespace Clinic_Booking.DTOs.UserDTO
{
    public class PaginationDto
    {
        public class PageResult<T>
        {
            public IEnumerable<T> Items { get; }
            public int TotalItems { get; }
            public int TotalPages { get; }
            public int CurrentPage { get; }
            public int PageSize { get; }
            public PageResult(IEnumerable<T> items, int totalItems, int totalPages, int currentPage, int pageSize)
            {
                Items = items;
                TotalItems = totalItems;
                TotalPages = totalPages;
                CurrentPage = currentPage;
                PageSize = pageSize;
            }
        }
    }
}
