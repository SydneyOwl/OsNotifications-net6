using Xunit;

namespace OsNotifications.Tests.E2E;

public class PlatformE2ETests
{
    [RequiresPlatform("Linux")]
    public void Linux_InitializeAndShowNotification_DoesNotThrow()
    {
        Notifications.SetApplicationName("OsNotifications.Tests");
        Notifications.Initialize();

        var exception = Record.Exception(() =>
            Notifications.ShowNotification("Test title", "Test message"));

        Assert.Null(exception);
    }

    [RequiresPlatform("Linux")]
    public async Task Linux_ShowNotificationAsync_DoesNotThrow()
    {
        Notifications.SetApplicationName("OsNotifications.Tests");
        Notifications.Initialize();

        var result = await Notifications.ShowNotificationAsync("Test title", "Test message");
        Assert.True(result);
    }

    [RequiresPlatform("Linux")]
    public void Linux_PermissionStatus_AfterInitialize_ReturnsGranted()
    {
        Notifications.SetApplicationName("OsNotifications.Tests");
        Notifications.Initialize();

        var status = Notifications.GetNotificationPermissionStatus();
        Assert.Equal(NotificationPermissionStatus.Granted, status);
    }


    [Fact(Skip = "macOS notifications require a bundle; test manually in an .app")]
    public void MacOS_RequestPermissionAndShowNotification_DoesNotThrow()
    {
        Notifications.RequestNotificationPermission();

        var exception = Record.Exception(() =>
            Notifications.ShowNotification("Test title", "Test message", "Informative text"));

        Assert.Null(exception);
    }
    
    [Fact(Skip = "macOS notifications require a bundle; test manually in an .app")]
    public async Task MacOS_ShowNotificationAsync_DoesNotThrow()
    {
        Notifications.RequestNotificationPermission();

        var result = await Notifications.ShowNotificationAsync("Test title", "Test message", "Informative text");
        Assert.True(result);
    }

    [Fact(Skip = "macOS notifications require a bundle; test manually in an .app")]
    public void MacOS_PermissionStatus_ReturnsValidEnum()
    {
        var status = Notifications.GetNotificationPermissionStatus();
        // Should be a valid enum value, not necessarily Granted
        Assert.True(Enum.IsDefined(status));
    }

    [RequiresPlatform("Windows")]
    public void Windows_InitializeAndShowNotification_DoesNotThrow()
    {
        Notifications.SetApplicationName("OsNotifications.Tests");
        Notifications.SetApplicationIdentifier("com.test.osnotifications");
        Notifications.Initialize();

        var exception = Record.Exception(() =>
            Notifications.ShowNotification("Test title", "Test message"));

        Assert.Null(exception);
    }

    [RequiresPlatform("Windows")]
    public async Task Windows_ShowNotificationAsync_DoesNotThrow()
    {
        Notifications.SetApplicationName("OsNotifications.Tests");
        Notifications.SetApplicationIdentifier("com.test.osnotifications");
        Notifications.Initialize();

        var result = await Notifications.ShowNotificationAsync("Test title", "Test message");
        Assert.True(result);
    }

    [RequiresPlatform("Windows")]
    public void Windows_PermissionStatus_ReturnsGrantedOrDenied()
    {
        Notifications.SetApplicationName("OsNotifications.Tests");
        Notifications.SetApplicationIdentifier("com.test.osnotifications");
        Notifications.Initialize();

        var status = Notifications.GetNotificationPermissionStatus();
        Assert.True(status is NotificationPermissionStatus.Granted or NotificationPermissionStatus.Denied);
    }

    [Fact]
    public void ShowNotification_EmptyTitle_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            Notifications.ShowNotification("", "message"));

        Assert.Null(exception);
    }

    [Fact]
    public void ShowNotification_EmptyMessage_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            Notifications.ShowNotification("title", ""));

        Assert.Null(exception);
    }
}