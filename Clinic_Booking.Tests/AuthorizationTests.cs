using Clinic_Booking.Authorization;
using Clinic_Booking.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Tests;

public class AuthorizationTests
{
    [Theory]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.GetAppointmentsAsync), AppRoles.SuperAdmin)]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.GetMineForDoctorAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.ToggleAppointmentStatusAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.CompleteAppointmentAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.CreateManualAppointmentAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(DoctorAvailabilityController), nameof(DoctorAvailabilityController.CreateOrUpdateWeeklyAvailabilityAsync), AppRoles.DoctorUser)]
    [InlineData(typeof(ReviewController), nameof(ReviewController.GetMineForDoctorAsync), AppRoles.DoctorUser)]
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

    [Theory]
    [InlineData(typeof(UserController), nameof(UserController.SignInAsync), "Auth")]
    [InlineData(typeof(UserController), nameof(UserController.RefreshTokenAsync), "Auth")]
    [InlineData(typeof(UserController), nameof(UserController.SendResetPasswordLinkAsync), "AccountRecovery")]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.CreateAppointmentAsync), "Booking")]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.ResendBookingOtpAsync), "Otp")]
    [InlineData(typeof(AppointmentController), nameof(AppointmentController.ConfirmBookingOtpAsync), "Otp")]
    public void AbuseProneEndpoint_HasRateLimitPolicy(Type controller, string action, string expectedPolicy)
    {
        var method = controller.GetMethod(action)!;
        var rateLimit = method.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true)
            .Cast<EnableRateLimitingAttribute>()
            .Single();

        Assert.Equal(expectedPolicy, rateLimit.PolicyName);
    }
}
