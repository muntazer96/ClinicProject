using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.RequestFrom
{
    public class RequestForm : BaseEntity<int>
    {
        public Guid? UserId { get; set; }
        public AspNetUsers? User { get; set; }

        public string PhoneNumber { get; set; }

        public string FullName { get; set; }
        public string KnownName { get; set; }

        public IraqiProvince IraqiProvince { get; set; }
        public DateOnly BirthDay { get; set; }

        public int SpecializationId { get; set; }
        public Specialization.Specialization Specialization { get; set; }

        public string IdentityFront { get; set; }
        public string? IdentityBack { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public string? RejectedReason { get; set; }

        public string Code { get; set; }

    }
    public enum RequestStatus
    {
        Waiting,
        Accepted,
        Rejected
    }
}
