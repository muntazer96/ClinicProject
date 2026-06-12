using Clinic_Booking.Data;
using Clinic_Booking.DTOs.ClinicExceptionDTO;
using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Entities.Clinic;
using Clinic_Booking.Entities.Day;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.SubscriptionPackage;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Services.AppointmentReschedulingServices;
using Clinic_Booking.Services.ClinicExceptionServices;
using Clinic_Booking.Services.DoctorAvailabilityServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clinic_Booking.Tests;

public class AppointmentMoveCapacityTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task ClinicExceptionMove_DistributesAppointmentsByTargetDayCapacity()
    {
        await using var context = CreateContext();
        var dates = SeedSchedule(context);
        SeedAppointments(context, dates.ExceptionDate, 10, queueStart: 1);
        SeedAppointments(context, dates.TargetDate, 9, queueStart: 1);
        await context.SaveChangesAsync();

        var service = new ClinicExceptionServices(
            context,
            new StubLoadServices(CurrentUserId),
            new StubPushNotificationServices(),
            new AppointmentReschedulingServices(context));
        var result = await service.UpsertMineAsync(new UpsertClinicExceptionDto
        {
            ClinicId = 1,
            ExceptionDate = DateOnly.FromDateTime(dates.ExceptionDate),
            IsClosed = true,
            AppointmentConflictAction = "move",
            MoveAppointmentsToDate = DateOnly.FromDateTime(dates.TargetDate)
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(15, await ActiveCountAsync(context, dates.TargetDate));
        Assert.Equal(4, await ActiveCountAsync(context, dates.NextDate));
        Assert.Equal(0, await ActiveCountAsync(context, dates.ExceptionDate));
        Assert.Equal(0, await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled));
        Assert.Equal([10, 11, 12, 13, 14, 15], await QueueNumbersAsync(context, dates.TargetDate, skipExisting: 9));
        Assert.Equal([1, 2, 3, 4], await QueueNumbersAsync(context, dates.NextDate));
    }

    [Fact]
    public async Task DisablingWorkingDay_DistributesAppointmentsByNextDaysCapacity()
    {
        await using var context = CreateContext();
        var dates = SeedSchedule(context);
        SeedAppointments(context, dates.ExceptionDate, 10, queueStart: 1);
        SeedAppointments(context, dates.TargetDate, 9, queueStart: 1);
        await context.SaveChangesAsync();

        var service = new DoctorAvailabilityService(
            context,
            new StubLoadServices(CurrentUserId),
            new StubPushNotificationServices(),
            new AppointmentReschedulingServices(context));
        var result = await service.UpdateSingleDayAvailabilityAsync(new UpdateSingleDayAvailabilityDto
        {
            ClinicId = 1,
            DayId = 1,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            MaxAppointments = 15,
            IsAvailable = false
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(15, await ActiveCountAsync(context, dates.TargetDate));
        Assert.Equal(4, await ActiveCountAsync(context, dates.NextDate));
        Assert.Equal(0, await ActiveCountAsync(context, dates.ExceptionDate));
        Assert.Equal(0, await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled));
    }

    [Fact]
    public async Task ClinicExceptionMove_CancelsAppointmentsWhenNoCapacityIsAvailable()
    {
        await using var context = CreateContext();
        var dates = SeedSchedule(context);
        SetFutureAvailabilityCapacity(context, maxAppointments: 0);
        SeedAppointments(context, dates.ExceptionDate, 3, queueStart: 1);
        await context.SaveChangesAsync();

        var pushNotifications = new StubPushNotificationServices();
        var service = new ClinicExceptionServices(
            context,
            new StubLoadServices(CurrentUserId),
            pushNotifications,
            new AppointmentReschedulingServices(context));

        var result = await service.UpsertMineAsync(new UpsertClinicExceptionDto
        {
            ClinicId = 1,
            ExceptionDate = DateOnly.FromDateTime(dates.ExceptionDate),
            IsClosed = true,
            AppointmentConflictAction = "move",
            MoveAppointmentsToDate = DateOnly.FromDateTime(dates.TargetDate)
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(3, await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled));
        Assert.Equal(0, await ActiveCountAsync(context, dates.ExceptionDate));
        Assert.Equal(3, await context.Notifications.CountAsync());
        Assert.Equal(3, pushNotifications.SentCount);
    }

    [Fact]
    public async Task ClinicExceptionMove_RejectsMoveDateOutsideClinicBookingWindow()
    {
        await using var context = CreateContext();
        var dates = SeedSchedule(context, bookingWindowDays: 1);
        SeedAppointments(context, dates.ExceptionDate, 1, queueStart: 1);
        SeedAppointments(context, dates.TargetDate, 15, queueStart: 1);
        await context.SaveChangesAsync();

        var pushNotifications = new StubPushNotificationServices();
        var service = new ClinicExceptionServices(
            context,
            new StubLoadServices(CurrentUserId),
            pushNotifications,
            new AppointmentReschedulingServices(context));

        var result = await service.UpsertMineAsync(new UpsertClinicExceptionDto
        {
            ClinicId = 1,
            ExceptionDate = DateOnly.FromDateTime(dates.ExceptionDate),
            IsClosed = true,
            AppointmentConflictAction = "move",
            MoveAppointmentsToDate = DateOnly.FromDateTime(dates.TargetDate)
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(15, await ActiveCountAsync(context, dates.TargetDate));
        Assert.Equal(0, await ActiveCountAsync(context, dates.NextDate));
        Assert.Equal(0, await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled));
        Assert.Equal(0, await context.Notifications.CountAsync());
        Assert.Equal(0, pushNotifications.SentCount);
    }

    [Fact]
    public async Task ClinicExceptionMove_RejectsMoveDateClosedByAnotherException()
    {
        await using var context = CreateContext();
        var dates = SeedSchedule(context);
        SeedAppointments(context, dates.ExceptionDate, 1, queueStart: 1);
        context.ClinicExceptions.Add(new Clinic_Booking.Entities.ClinicException.ClinicException
        {
            ClinicId = 1,
            ExceptionDate = dates.TargetDate,
            IsClosed = true,
            ClosureReason = "Closed target date"
        });
        await context.SaveChangesAsync();

        var pushNotifications = new StubPushNotificationServices();
        var service = new ClinicExceptionServices(
            context,
            new StubLoadServices(CurrentUserId),
            pushNotifications,
            new AppointmentReschedulingServices(context));

        var result = await service.UpsertMineAsync(new UpsertClinicExceptionDto
        {
            ClinicId = 1,
            ExceptionDate = DateOnly.FromDateTime(dates.ExceptionDate),
            IsClosed = true,
            AppointmentConflictAction = "move",
            MoveAppointmentsToDate = DateOnly.FromDateTime(dates.TargetDate)
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(1, await ActiveCountAsync(context, dates.ExceptionDate));
        Assert.Equal(0, await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled));
        Assert.Equal(0, await context.Notifications.CountAsync());
        Assert.Equal(0, pushNotifications.SentCount);
    }

    [Fact]
    public async Task DisablingWorkingDay_CancelsAppointmentsWhenNoCapacityIsAvailable()
    {
        await using var context = CreateContext();
        var dates = SeedSchedule(context);
        SetFutureAvailabilityCapacity(context, maxAppointments: 0);
        SeedAppointments(context, dates.ExceptionDate, 3, queueStart: 1);
        await context.SaveChangesAsync();

        var pushNotifications = new StubPushNotificationServices();
        var service = new DoctorAvailabilityService(
            context,
            new StubLoadServices(CurrentUserId),
            pushNotifications,
            new AppointmentReschedulingServices(context));

        var result = await service.UpdateSingleDayAvailabilityAsync(new UpdateSingleDayAvailabilityDto
        {
            ClinicId = 1,
            DayId = 1,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            MaxAppointments = 15,
            IsAvailable = false
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(3, await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled));
        Assert.Equal(0, await ActiveCountAsync(context, dates.ExceptionDate));
        Assert.Equal(3, await context.Notifications.CountAsync());
        Assert.Equal(3, pushNotifications.SentCount);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (DateTime ExceptionDate, DateTime TargetDate, DateTime NextDate) SeedSchedule(
        ApplicationDbContext context,
        int bookingWindowDays = 31)
    {
        var exceptionDate = Next(DayOfWeek.Monday);
        var targetDate = exceptionDate.AddDays(1);
        var nextDate = exceptionDate.AddDays(2);

        context.Doctors.Add(new Doctor
        {
            Id = 1,
            UserId = CurrentUserId,
            Name = "Doctor",
            NormalizedName = "DOCTOR",
            Description = "Test doctor",
            ImageName = "default.png"
        });
        context.Clinics.Add(new Clinic
        {
            Id = 1,
            DoctorId = 1,
            Name = "Clinic",
            Address = "Address",
            BookingWindowDays = bookingWindowDays
        });
        context.SubscriptionPackages.Add(new SubscriptionPackage
        {
            Id = 1,
            Name = "Gold",
            NormalizedName = "GOLD",
            MaxClinics = 5,
            MaxDailyAppointments = 30,
            MaxWeeklyDays = 7
        });
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorId = 1,
            PackageId = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = SubscriptionStatus.Active
        });
        context.Days.AddRange(
            CreateDay(1, exceptionDate.DayOfWeek),
            CreateDay(2, targetDate.DayOfWeek),
            CreateDay(3, nextDate.DayOfWeek));
        context.DoctorAvailabilities.AddRange(
            CreateAvailability(1, 1, isAvailable: true, maxAppointments: 15),
            CreateAvailability(2, 2, isAvailable: true, maxAppointments: 15),
            CreateAvailability(3, 3, isAvailable: true, maxAppointments: 15));

        return (exceptionDate, targetDate, nextDate);
    }

    private static void SetFutureAvailabilityCapacity(ApplicationDbContext context, int maxAppointments)
    {
        foreach (var availability in context.ChangeTracker
                     .Entries<DoctorAvailability>()
                     .Select(entry => entry.Entity)
                     .Where(item => !item.IsDeleted))
        {
            availability.MaxAppointments = maxAppointments;
        }
    }

    private static Day CreateDay(int id, DayOfWeek dayOfWeek)
    {
        return new Day
        {
            Id = id,
            Name = dayOfWeek.ToString(),
            NormalizedName = dayOfWeek.ToString()
        };
    }

    private static DoctorAvailability CreateAvailability(int id, int dayId, bool isAvailable, int maxAppointments)
    {
        return new DoctorAvailability
        {
            Id = id,
            ClinicId = 1,
            DayId = dayId,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            MaxAppointments = maxAppointments,
            IsAvailable = isAvailable
        };
    }

    private static void SeedAppointments(ApplicationDbContext context, DateTime date, int count, int queueStart)
    {
        for (var i = 0; i < count; i++)
        {
            context.Appointments.Add(new Appointment
            {
                DoctorId = 1,
                ClinicId = 1,
                AppointmentDate = date.Date,
                QueueNumber = queueStart + i,
                Status = AppointmentStatus.Pending,
                Code = $"{date:yyyyMMdd}-{queueStart + i}",
                UserId = Guid.NewGuid()
            });
        }
    }

    private static DateTime Next(DayOfWeek dayOfWeek)
    {
        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }

        return date.Date;
    }

    private static Task<int> ActiveCountAsync(ApplicationDbContext context, DateTime date)
    {
        return context.Appointments.CountAsync(appointment =>
            appointment.AppointmentDate == date.Date &&
            appointment.Status != AppointmentStatus.Cancelled &&
            appointment.Status != AppointmentStatus.Completed &&
            !appointment.IsDeleted);
    }

    private static Task<List<int>> QueueNumbersAsync(ApplicationDbContext context, DateTime date, int skipExisting = 0)
    {
        return context.Appointments
            .Where(appointment => appointment.AppointmentDate == date.Date)
            .OrderBy(appointment => appointment.QueueNumber)
            .Skip(skipExisting)
            .Select(appointment => appointment.QueueNumber)
            .ToListAsync();
    }

    private sealed class StubLoadServices(Guid userId) : ILoadServices
    {
        public Guid? GetCurrentUserId() => userId;
        public string SandEmailHTMLTemplate(string confirmationLink) => confirmationLink;
        public string ResetPasswordHTMLTemplate(string resetLink) => resetLink;
    }

    private sealed class StubPushNotificationServices : IPushNotificationServices
    {
        public int SentCount { get; private set; }

        public Task SendToUserAsync(
            Guid userId,
            string title,
            string body,
            IDictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            SentCount++;
            return Task.CompletedTask;
        }
    }
}
