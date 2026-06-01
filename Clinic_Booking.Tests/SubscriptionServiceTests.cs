using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.Services.DoctorSubscriptionServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Tests;

public class SubscriptionServiceTests
{
    [Fact]
    public async Task CreateSubscription_RejectsUnsupportedInitialStatus()
    {
        await using var context = CreateContext();
        context.Doctors.Add(CreateDoctor());
        await context.SaveChangesAsync();

        var service = new DoctorSubscriptionServices(context, new StubLoadServices());
        var result = await service.CreateSubscriptionAsync(new DoctorSubscriptionAddDto
        {
            DoctorId = 1,
            PackageId = 1,
            Status = SubscriptionStatus.Expired
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateSubscription_RejectsDuplicateActiveSubscription()
    {
        await using var context = CreateContext();
        context.Doctors.Add(CreateDoctor());
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorId = 1,
            PackageId = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = SubscriptionStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new DoctorSubscriptionServices(context, new StubLoadServices());
        var result = await service.CreateSubscriptionAsync(new DoctorSubscriptionAddDto
        {
            DoctorId = 1,
            PackageId = 1,
            Status = SubscriptionStatus.Active
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Doctor CreateDoctor()
    {
        return new Doctor
        {
            Id = 1,
            Name = "Doctor",
            NormalizedName = "DOCTOR",
            Description = "Test doctor",
            ImageName = "default.png"
        };
    }

    private sealed class StubLoadServices : ILoadServices
    {
        public Guid? GetCurrentUserId() => Guid.Empty;
        public string SandEmailHTMLTemplate(string confirmationLink) => confirmationLink;
        public string ResetPasswordHTMLTemplate(string resetLink) => resetLink;
    }
}
