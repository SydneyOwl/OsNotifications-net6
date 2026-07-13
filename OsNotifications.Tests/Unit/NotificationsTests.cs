using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;
using OsNotifications;
using OsNotifications.Platforms.Mac;

namespace OsNotifications.Tests.Unit;

public class NotificationsTests
{
    [Fact]
    public void SetApplicationName_TrimsAndStores()
    {
        Notifications.SetApplicationName("  My App  ");
        var result = Notifications.GetApplicationName();
        Assert.Equal("My App", result);
    }

    [Fact]
    public void SetApplicationName_EmptyString_FallsBackToEntryAssembly()
    {
        Notifications.SetApplicationName("");
        var result = Notifications.GetApplicationName();
        var expected = Assembly.GetEntryAssembly()?.GetName().Name ?? "";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SetApplicationIdentifier_TrimsAndStores()
    {
        Notifications.SetApplicationIdentifier("  com.example.app  ");
        var result = Notifications.GetApplicationIdentifier();
        Assert.Equal("com.example.app", result);
    }

    [Fact]
    public void SetApplicationIdentifier_EmptyString_FallsBackToApplicationName()
    {
        Notifications.SetApplicationName("FallbackApp");
        Notifications.SetApplicationIdentifier("");
        var result = Notifications.GetApplicationIdentifier();
        Assert.Equal("FallbackApp", result);
    }

    [Fact]
    public void GetApplicationIdentifier_WhenBothUnset_FallsBackToEntryAssembly()
    {
        Notifications.SetApplicationName("");
        Notifications.SetApplicationIdentifier("");
        var result = Notifications.GetApplicationIdentifier();
        var expected = Assembly.GetEntryAssembly()?.GetName().Name ?? "";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RequestNotificationPermission_OnNonMacOS_ReturnsTrue()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return; // skip on macOS, test is for non-macOS

        var result = Notifications.RequestNotificationPermission();
        Assert.True(result);
    }

    [Fact]
    public async Task RequestNotificationPermissionAsync_OnNonMacOS_ReturnsTrue()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var result = await Notifications.RequestNotificationPermissionAsync();
        Assert.True(result);
    }

    [Fact]
    public void GetNotificationPermissionStatus_OnNonMacOS_ReturnsGrantedOrDenied()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;

        var status = Notifications.GetNotificationPermissionStatus();
        // On Windows/Linux, should be Granted or Denied, never NotDetermined or Error
        Assert.True(status is NotificationPermissionStatus.Granted or NotificationPermissionStatus.Denied);
    }
}