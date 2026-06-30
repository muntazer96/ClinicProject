namespace Clinic_Booking.DTOs.DoctorAvailabilityDTO
{
    public class GetDoctorDayAvailabilityDto
    {
        public int? Id { get; set; }
        public int ClinicId { get; set; }
        public int DayId { get; set; } // رقم اليوم من الأسبوع
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } // اسم اليوم (اختياري)
        public string DayNormailzedName { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? MaxAppointments { get; set; }
    }
}
