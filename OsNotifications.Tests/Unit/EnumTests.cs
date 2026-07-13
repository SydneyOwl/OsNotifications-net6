using Xunit;
using OsNotifications;
using OsNotifications.Platforms.Mac;

namespace OsNotifications.Tests.Unit;

public class EnumTests
{
    [Fact]
    public void NotificationPermissionStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)NotificationPermissionStatus.NotDetermined);
        Assert.Equal(1, (int)NotificationPermissionStatus.Denied);
        Assert.Equal(2, (int)NotificationPermissionStatus.Granted);
        Assert.Equal(-1, (int)NotificationPermissionStatus.Error);
    }

    [Fact]
    public void MacNotificationPermissionStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)MacNotificationPermissionStatus.NotDetermined);
        Assert.Equal(1, (int)MacNotificationPermissionStatus.Denied);
        Assert.Equal(2, (int)MacNotificationPermissionStatus.Authorized);
        Assert.Equal(3, (int)MacNotificationPermissionStatus.Provisional);
        Assert.Equal(4, (int)MacNotificationPermissionStatus.Ephemeral);
        Assert.Equal(-1, (int)MacNotificationPermissionStatus.Error);
    }

    [Theory]
    [InlineData(MacNotificationPermissionStatus.Authorized, NotificationPermissionStatus.Granted)]
    [InlineData(MacNotificationPermissionStatus.Provisional, NotificationPermissionStatus.Granted)]
    [InlineData(MacNotificationPermissionStatus.Ephemeral, NotificationPermissionStatus.Granted)]
    [InlineData(MacNotificationPermissionStatus.Denied, NotificationPermissionStatus.Denied)]
    [InlineData(MacNotificationPermissionStatus.NotDetermined, NotificationPermissionStatus.NotDetermined)]
    [InlineData(MacNotificationPermissionStatus.Error, NotificationPermissionStatus.Error)]
    public void MapMacStatus_MapsCorrectly(MacNotificationPermissionStatus input, NotificationPermissionStatus expected)
    {
        var result = Notifications.MapMacStatus(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MapMacStatus_UnknownValue_ReturnsError()
    {
        var result = Notifications.MapMacStatus((MacNotificationPermissionStatus)999);
        Assert.Equal(NotificationPermissionStatus.Error, result);
    }
}