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
    /// Checks whether notification permission is currently granted.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the app is authorized to show notifications; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <b>macOS:</b> Checks <c>UNAuthorizationStatus</c> — returns <see langword="true"/> for
    /// Authorized, Provisional, or Ephemeral.
    /// <br/>
    /// <b>Windows:</b> Reads the <c>Enabled</c> registry value under
    /// <c>HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\&lt;AUMID&gt;</c>.
    /// Returns <see langword="true"/> if the key is absent or the value is non-zero.
    /// <br/>
    /// <b>Linux:</b> Returns <see langword="true"/> if the DBus notification service is available.
    /// The FreeDesktop Notifications specification has no permission model — individual daemons
    /// may filter or suppress notifications at their own discretion, which cannot be queried.
    /// </remarks>
    public static bool IsNotificationPermissionGranted() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacNotifications.IsPermissionGranted();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return WindowsNotifications.IsPermissionGranted();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LinuxNotifications.IsPermissionGranted();

        return false;
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
            WindowsNotifications.Show(title, message);
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
            return WindowsNotifications.ShowAsync(title, message);

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