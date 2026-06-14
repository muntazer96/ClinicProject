namespace Clinic_Booking.Entities.Shared
{
    public class BaseEntity<T>
    {
        public T? Id { get; set; }
        public Guid? DeleterId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Guid? CreatorId { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? ModifierId { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
