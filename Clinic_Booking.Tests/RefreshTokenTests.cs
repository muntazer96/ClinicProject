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
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        Assert.True(token.IsActive);

        token.RevokedAt = DateTime.UtcNow;
        Assert.False(token.IsActive);

        token.RevokedAt = null;
        token.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        Assert.False(token.IsActive);

        token.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
        token.IsDeleted = true;
        Assert.False(token.IsActive);
    }
}
