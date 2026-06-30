using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Referral
{
    public class Referral : BaseEntity<int>
    {
        public int InviterDoctorId { get; set; } // الدكتور الذي قام بالإحالة
        public Doctor.Doctor InviterDoctor { get; set; }

        public int InvitedDoctorId { get; set; } // الدكتور الذي تم إحالةه
        public Doctor.Doctor InvitedDoctor { get; set; }

        public DateTime ReferredAt { get; set; } // وقت الإحالة
    }
}
