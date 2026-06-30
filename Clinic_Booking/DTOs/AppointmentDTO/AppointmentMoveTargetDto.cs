namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public sealed record AppointmentMoveTargetDto(DateTime Date, int QueueNumber);

    public sealed class AppointmentMoveCapacityState(int bookedCount, int nextQueueNumber)
    {
        public int BookedCount { get; set; } = bookedCount;
        public int NextQueueNumber { get; set; } = nextQueueNumber;
    }
}
