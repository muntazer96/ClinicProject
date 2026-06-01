using Clinic_Booking.Authorization;
using Clinic_Booking.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Clinic_Booking.Tests;

public class AuthorizationTests
{
    [Theory]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.GetAppointmentsAsync), AppRoles.SuperAdmin)]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.ToggleAppointmentStatusAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.CompleteAppointmentAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(DoctorAvailabilityController), nameof(DoctorAvailabilityController.CreateOrUpdateWeeklyAvailabilityAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(UserController), nameof(UserController.GetUsersAsync), AppRoles.SuperAdmin)]
    public void SensitiveEndpoint_RequiresExpectedRole(Type controller, string action, string expectedRole)
    {
        var method = controller.GetMethod(action)!;
        var authorize = method.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(expectedRole, authorize.Roles);
    }

    [Fact]
    public void SubscriptionController_RequiresSuperAdmin()
    {
        var authorize = typeof(DoctorSubscriptionController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(AppRoles.SuperAdmin, authorize.Roles);
    }

    [Fact]
    public void GuestBooking_RemainsPublic()
    {
        var method = typeof(AppointmentController)
            .GetMethod(nameof(AppointmentController.CreateAppointmentAsync))!;

        Assert.Empty(method.GetCustomAttributes(typeof(AuthorizeAttribute), true));
    }
}
