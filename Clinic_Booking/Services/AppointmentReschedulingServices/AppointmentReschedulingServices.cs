using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IAppointmentReschedulingServices;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.AppointmentReschedulingServices
{
    public class AppointmentReschedulingServices : IAppointmentReschedulingServices
    {
        private const int DefaultSearchWindowDays = 7;
        private readonly ApplicationDbContext _context;

        public AppointmentReschedulingServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentMoveTargetDto?> FindNextMoveTargetAsync(
            int clinicId,
            DateTime startDate,
            Dictionary<DateTime, AppointmentMoveCapacityState> moveStates)
        {
            var availableDays = await _context.DoctorAvailabilities
                .Include(availability => availability.Day)
                .Where(availability =>
                    availability.ClinicId == clinicId &&
                    availability.IsAvailable &&
                    !availability.IsDeleted)
                .ToListAsync();

            if (availableDays.Count == 0)
            {
                return null;
            }

            var searchWindowDays = await GetSearchWindowDaysAsync(clinicId);
            for (var offset = 0; offset < searchWindowDays; offset++)
            {
                var candidate = startDate.AddDays(offset).Date;
                var availability = availableDays.FirstOrDefault(item =>
                    item.Day.NormalizedName == candidate.DayOfWeek.ToString());
                if (availability == null)
                {
                    continue;
                }

                var clinicException = await _context.ClinicExceptions
                    .FirstOrDefaultAsync(exception =>
                        exception.ClinicId == clinicId &&
                        exception.ExceptionDate == candidate &&
                        !exception.IsDeleted);
                if (clinicException?.IsClosed == true)
                {
                    continue;
                }

                var maxAppointments = clinicException?.MaxAppointments ?? availability.MaxAppointments;
                if (maxAppointments <= 0)
                {
                    continue;
                }

                if (!moveStates.TryGetValue(candidate, out var state))
                {
                    var existingCount = await _context.Appointments.CountAsync(appointment =>
                        appointment.ClinicId == clinicId &&
                        appointment.AppointmentDate.Date == candidate &&
                        appointment.Status != AppointmentStatus.Cancelled &&
                        appointment.Status != AppointmentStatus.Completed &&
                        !appointment.IsDeleted);
                    var maxQueueNumber = await _context.Appointments
                        .Where(appointment =>
                            appointment.ClinicId == clinicId &&
                            appointment.AppointmentDate.Date == candidate &&
                            !appointment.IsDeleted)
                        .Select(appointment => (int?)appointment.QueueNumber)
                        .MaxAsync() ?? 0;

                    state = new AppointmentMoveCapacityState(existingCount, maxQueueNumber);
                    moveStates[candidate] = state;
                }

                if (state.BookedCount >= maxAppointments)
                {
                    continue;
                }

                state.BookedCount++;
                state.NextQueueNumber++;
                return new AppointmentMoveTargetDto(candidate, state.NextQueueNumber);
            }

            return null;
        }

        private async Task<int> GetSearchWindowDaysAsync(int clinicId)
        {
            var bookingWindowDays = await _context.Clinics
                .Where(clinic => clinic.Id == clinicId && !clinic.IsDeleted)
                .Select(clinic => clinic.BookingWindowDays)
                .FirstOrDefaultAsync();

            return bookingWindowDays <= 0 ? DefaultSearchWindowDays : bookingWindowDays;
        }
    }
}
