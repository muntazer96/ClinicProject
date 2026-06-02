namespace Clinic_Booking.DTOs.UserDTO
{
    public class UserGetDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ImageName { get; set; }
        public bool IsLocked { get; set; }
        public bool IsFirstLogin { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public string? RoleName { get; set; }
        public Guid? RoleId { get; set; }
    }
}
