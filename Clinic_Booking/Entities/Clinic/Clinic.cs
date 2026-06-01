using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Clinic
{
    public class Clinic : BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }
        public string Name { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVisible { get; set; } = true;
        public ICollection<DoctorAvailability.DoctorAvailability> Availabilities { get; set; }
        public ICollection<Appointment.Appointment> Appointments { get; set; }
        public ICollection<ClinicException.ClinicException> Exceptions { get; set; }
    }
}
