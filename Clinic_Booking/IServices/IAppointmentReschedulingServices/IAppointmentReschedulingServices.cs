using Clinic_Booking.DTOs.AppointmentDTO;

namespace Clinic_Booking.IServices.IAppointmentReschedulingServices
{
    public interface IAppointmentReschedulingServices
    {
        Task<AppointmentMoveTargetDto?> FindNextMoveTargetAsync(
            int clinicId,
            DateTime startDate,
            Dictionary<DateTime, AppointmentMoveCapacityState> moveStates);
    }
}
