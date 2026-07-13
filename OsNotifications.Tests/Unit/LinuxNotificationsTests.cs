using Xunit;
using OsNotifications.Platforms.Linux;

namespace OsNotifications.Tests.Unit;

public class LinuxNotificationsTests
{
    [Fact]
    public void IsPermissionGranted_BeforeInitialize_ReturnsFalse()
    {
        // Create a fresh instance by checking before initialization
        // Since LinuxNotifications is a static class, we verify the initial state
        var result = LinuxNotifications.IsPermissionGranted();
        // On Linux without DBus or on non-Linux, this should be false
        Assert.False(result);
    }
}