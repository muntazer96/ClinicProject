namespace Clinic_Booking.DTOs.ClinicExceptionDTO
{
    public class GetClinicExceptionDto
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public DateOnly ExceptionDate { get; set; }
        public bool IsClosed { get; set; }
        public string? ClosureReason { get; set; }
        public int? MaxAppointments { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
