using Clinic_Booking.Entities.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_Booking.Entities.Review
{
    public class Review : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public User.AspNetUsers User { get; set; }

        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        [Column("AppoinmentId")]
        public int? AppointmentId { get; set; }
        public Appointment.Appointment? Appointment { get; set; }
    }
}
