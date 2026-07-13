using System.Runtime.InteropServices;
using System.Reflection;
using OsNotifications.Platforms.Linux;
using OsNotifications.Platforms.Mac;
using OsNotifications.Platforms.Windows;

namespace OsNotifications;

/// <summary>
/// Cross-platform notification permission status.
/// </summary>
public enum NotificationPermissionStatus {
    /// <summary>Permission has not been requested yet (macOS), or not applicable.</summary>
    NotDetermined = 0,
    /// <summary>User or system has denied notification permission.</summary>
    Denied = 1,
    /// <summary>Notifications are authorized.</summary>
    Granted = 2,
    /// <summary>Unable to determine the status due to an error or timeout.</summary>
    Error = -1
}

public partial class Notifications {
    private static string? _applicationIdentifier;
    private static string? _applicationName;

    /// <summary>
    /// Sets the application identifier used by Windows (AUMID).
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

    /// <summary>
    /// Initializes the notification system for the current platform.
    /// </summary>
    /// <remarks>
    /// Call this once at app startup, before any calls to <see cref="ShowNotification"/>.
    /// <br/>
    /// <b>Windows:</b> Registers the AUMID and creates a Start Menu shortcut.
    /// <br/>
    /// <b>Linux:</b> Connects to the FreeDesktop notification DBus service.
    /// <br/>
    /// <b>macOS:</b> No-op (use <see cref="RequestNotificationPermission"/> instead).
    /// </remarks>
    public static void Initialize() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            WindowsNotifications.Initialize(GetApplicationName(), GetApplicationIdentifier());
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            LinuxNotifications.Initialize();
    }

    /// <summary>
    /// Requests notification authorization from the user.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the user granted authorization; <see langword="false"/> if denied or an error occurred.
    /// </returns>
    /// <remarks>
    /// On macOS, call this before any calls to <see cref="ShowNotification"/>.
    /// The system prompts the user only once — on the very first call — and stores the response
    /// permanently. Subsequent calls (even across app restarts) return immediately without prompting.
    /// On other platforms, this is a no-op and always returns <see langword="true"/>.
    /// </remarks>
    public static bool RequestNotificationPermission() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.RequestPermission();
        return true;
    }

    /// <inheritdoc cref="RequestNotificationPermission"/>
    public static Task<bool> RequestNotificationPermissionAsync() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.RequestPermissionAsync();
        return Task.FromResult(true);
    }

    /// <summary>
    /// Returns the current notification permission status.
    /// </summary>
    /// <remarks>
    /// <b>macOS:</b> Maps directly from <c>UNAuthorizationStatus</c>.
    /// <b>Windows:</b> <see cref="NotificationPermissionStatus.Granted"/> if the registry
    /// <c>Enabled</c> value is non-zero or absent; <see cref="NotificationPermissionStatus.Denied"/> otherwise.
    /// <b>Linux:</b> <see cref="NotificationPermissionStatus.Granted"/> if the DBus service is available.
    /// </remarks>
    public static NotificationPermissionStatus GetNotificationPermissionStatus() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MapMacStatus(MacNotifications.GetPermissionStatus());
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return WindowsNotifications.IsPermissionGranted()
                ? NotificationPermissionStatus.Granted
                : NotificationPermissionStatus.Denied;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNotifications.IsPermissionGranted()
                ? NotificationPermissionStatus.Granted
                : NotificationPermissionStatus.Denied;

        return NotificationPermissionStatus.Error;
    }

    /// <inheritdoc cref="GetNotificationPermissionStatus"/>
    public static Task<NotificationPermissionStatus> GetNotificationPermissionStatusAsync() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.GetPermissionStatusAsync()
                .ContinueWith(t => MapMacStatus(t.Result));
        return Task.FromResult(GetNotificationPermissionStatus());
    }

    /// <summary>
    /// Shows a native OS notification.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the notification was delivered successfully; <see langword="false"/> on error or timeout.
    /// </returns>
    /// <remarks>
    /// On macOS, call <see cref="RequestNotificationPermission"/> first to request authorization.
    /// Without it, the system may silently drop the notification.
    /// </remarks>
    public static bool ShowNotification(string title, string message = "", string informativeText = "") {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            LinuxNotifications.Show(title, message, GetApplicationName());
            return true;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.Show(title, message, informativeText);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            WindowsNotifications.Show(title, message);
            return true;
        }

        throw new PlatformNotSupportedException("Notifications are only supported on Linux, MacOS and Windows");
    }

    /// <inheritdoc cref="ShowNotification"/>
    public static Task<bool> ShowNotificationAsync(string title, string message = "", string informativeText = "") {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return WrapAsync(LinuxNotifications.ShowAsync(title, message, GetApplicationName()));
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.ShowAsync(title, message, informativeText);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return WrapAsync(WindowsNotifications.ShowAsync(title, message));

        throw new PlatformNotSupportedException("Notifications are only supported on Linux, MacOS and Windows");
    }

    private static async Task<bool> WrapAsync(Task task) {
        await task.ConfigureAwait(false);
        return true;
    }

    private static NotificationPermissionStatus MapMacStatus(MacNotificationPermissionStatus macStatus) {
        return macStatus switch {
            MacNotificationPermissionStatus.Authorized => NotificationPermissionStatus.Granted,
            MacNotificationPermissionStatus.Provisional => NotificationPermissionStatus.Granted,
            MacNotificationPermissionStatus.Ephemeral => NotificationPermissionStatus.Granted,
            MacNotificationPermissionStatus.Denied => NotificationPermissionStatus.Denied,
            MacNotificationPermissionStatus.NotDetermined => NotificationPermissionStatus.NotDetermined,
            _ => NotificationPermissionStatus.Error
        };
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