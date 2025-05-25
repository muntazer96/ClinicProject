using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.DoctorAvailability
{
    public class DoctorAvailability : BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public Doctor.Doctor Doctor { get; set; }

        public int DayId { get; set; }
        public Day.Day Day { get; set; }

        public TimeSpan StartTime { get; set; } // بداية الفترة
        public TimeSpan EndTime { get; set; } // نهاية الفترة

        public int MaxAppointments { get; set; } // أقصى عدد حجوزات في هذه الفترة

        public bool IsAvailable { get; set; } = true; // هل الفترة متاحة للحجز
    }
}
