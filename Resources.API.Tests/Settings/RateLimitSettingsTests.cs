using Resources.API.Settings;
using Xunit;

namespace Resources.API.Tests.Settings;

public class RateLimitSettingsTests
{
    [Fact]
    public void RateLimitSettings_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var settings = new RateLimitSettings
        {
            PermitLimit = 10,
            Window = 60,
            QueueLimit = 5
        };

        // Assert
        Assert.Equal(10, settings.PermitLimit);
        Assert.Equal(60, settings.Window);
        Assert.Equal(5, settings.QueueLimit);
    }

    [Fact]
    public void RateLimitSettings_DefaultValues_AreCorrect()
    {
        // Arrange
        var settings = new RateLimitSettings();

        // Assert
        Assert.Equal(100, settings.PermitLimit);
        Assert.Equal(10, settings.Window);
        Assert.Equal(2, settings.QueueLimit);
    }
} 