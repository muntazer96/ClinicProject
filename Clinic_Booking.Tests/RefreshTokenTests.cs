using Clinic_Booking.Entities.RefreshToken;

namespace Clinic_Booking.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_IsActiveOnlyWhenNotExpiredRevokedOrDeleted()
    {
        var token = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiresAt = BusinessClock.Now().AddMinutes(5)
        };

        Assert.True(token.IsActive);

        token.RevokedAt = BusinessClock.Now();
        Assert.False(token.IsActive);

        token.RevokedAt = null;
        token.ExpiresAt = BusinessClock.Now().AddMinutes(-1);
        Assert.False(token.IsActive);

        token.ExpiresAt = BusinessClock.Now().AddMinutes(5);
        token.IsDeleted = true;
        Assert.False(token.IsActive);
    }
}
