using System.Runtime.InteropServices;
using System.Reflection;
using OsNotifications.Platforms.Linux;
using OsNotifications.Platforms.Mac;
using OsNotifications.Platforms.Windows;

namespace OsNotifications;

public partial class Notifications {
    private static string? _applicationIdentifier;
    private static string? _applicationName;

    /// <summary>
    /// Sets the application identifier used by Windows (AUMID) and previously by macOS.
    /// On macOS, the real bundle identifier from the app bundle is used automatically.
    /// </summary>
    public static void SetApplicationIdentifier(string identifier) {
        _applicationIdentifier = identifier;
    }

    /// <summary>
    /// Sets the application display name used by Windows (Start Menu shortcut) and Linux (FreeDesktop app name).
    /// </summary>
    public static void SetApplicationName(string name) {
        _applicationName = name;
    }

    static Notifications() {
        LinuxNotifications.Initialize();
    }

    /// <summary>
    /// Requests notification authorization from the user.
    /// </summary>
    /// <remarks>
    /// On macOS, call this before any calls to <see cref="ShowNotification"/>.
    /// The system prompts the user only once — on the very first call — and stores the response
    /// permanently. Subsequent calls (even across app restarts) return immediately without prompting.
    /// If permission is not granted, notifications will be silently ignored by the system.
    /// On other platforms, this is a no-op.
    /// </remarks>
    public static void RequestNotificationPermission() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            MacNotifications.RequestPermission();
    }

    /// <summary>
    /// Shows a native OS notification.
    /// </summary>
    /// <remarks>
    /// On macOS, call <see cref="RequestNotificationPermission"/> first to request authorization.
    /// Without it, the system may silently drop the notification.
    /// </remarks>
    public static void ShowNotification(string title, string message = "", string informativeText = "") {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            LinuxNotifications.Show(title, message, GetApplicationName());
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            MacNotifications.Show(title, message, informativeText);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            WindowsNotifications.Show(title, message, GetApplicationName(), GetApplicationIdentifier());
        else
            throw new PlatformNotSupportedException("Notifications are only supported on Linux, MacOS and Windows");
    }

    /// <inheritdoc cref="ShowNotification"/>
    public static Task ShowNotificationAsync(string title, string message = "", string informativeText = "") {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNotifications.ShowAsync(title, message, GetApplicationName());
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.ShowAsync(title, message, informativeText);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return WindowsNotifications.ShowAsync(title, message, GetApplicationName(), GetApplicationIdentifier());

        throw new PlatformNotSupportedException("Notifications are only supported on Linux, MacOS and Windows");
    }

    private static string GetApplicationName() {
        if (!string.IsNullOrWhiteSpace(_applicationName))
            return _applicationName.Trim();

        return Assembly.GetEntryAssembly()?.GetName().Name ?? "";
    }

    private static string GetApplicationIdentifier() {
        if (!string.IsNullOrWhiteSpace(_applicationIdentifier))
            return _applicationIdentifier.Trim();

        return GetApplicationName();
    }
}