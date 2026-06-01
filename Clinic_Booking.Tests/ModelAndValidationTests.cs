using System.ComponentModel.DataAnnotations;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.Entities.Appointment;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Tests;

public class ModelAndValidationTests
{
    [Fact]
    public void AppointmentModel_HasQueueAndLookupIndexes()
    {
        using var context = CreateContext();
        var indexes = context.Model.FindEntityType(typeof(Appointment))!
            .GetIndexes()
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToHashSet();

        Assert.Contains("ClinicId,AppointmentDate,QueueNumber", indexes);
        Assert.Contains("ClinicId,AppointmentDate,Status", indexes);
        Assert.Contains("UserId,AppointmentDate,Status", indexes);
        Assert.Contains("GuestPhoneNumber,AppointmentDate,Status", indexes);
    }

    [Fact]
    public void AppointmentModel_QueueIndexIsUnique()
    {
        using var context = CreateContext();
        var queueIndex = context.Model.FindEntityType(typeof(Appointment))!
            .GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual(["ClinicId", "AppointmentDate", "QueueNumber"]));

        Assert.True(queueIndex.IsUnique);
    }

    [Fact]
    public void BookingInput_RejectsInvalidIdsAndGuestPhone()
    {
        var form = new AddAppointmentDto
        {
            DoctorId = 0,
            ClinicId = 0,
            AppointmentDate = DateTime.Today,
            GuestPhoneNumber = "not-a-phone"
        };

        Assert.False(IsValid(form));
    }

    [Fact]
    public void SubscriptionInput_RejectsMissingDoctorAndPackage()
    {
        Assert.False(IsValid(new DoctorSubscriptionAddDto()));
        Assert.False(IsValid(new UpgradeDoctorSubscriptionDto()));
    }

    [Fact]
    public void ReviewInput_RejectsRatingOutsideRange()
    {
        var form = new AddReviewDto
        {
            DoctorId = 1,
            AppointmentId = 1,
            Rating = 6,
            Comment = "Invalid rating"
        };

        Assert.False(IsValid(form));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=clinic_booking_model_tests;Username=test;Password=test")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static bool IsValid(object model)
    {
        return Validator.TryValidateObject(model, new ValidationContext(model), [], true);
    }
}
