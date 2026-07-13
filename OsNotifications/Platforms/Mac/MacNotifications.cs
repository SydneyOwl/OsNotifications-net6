using System.Runtime.InteropServices;

namespace OsNotifications.Platforms.Mac;

internal static class MacNotifications {
    [DllImport("macNotification.dylib")]
    private static extern bool requestNotificationPermission();

    [DllImport("macNotification.dylib")]
    private static extern bool isNotificationPermissionGranted();

    [DllImport("macNotification.dylib")]
    private static extern void showNotification(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string title,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string subtitle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string body);

    public static void RequestPermission() {
        requestNotificationPermission();
    }

    public static bool IsPermissionGranted() {
        return isNotificationPermissionGranted();
    }

    public static void Show(string title, string message, string informativeText) {
        showNotification(title, message, informativeText);
    }

    public static Task ShowAsync(string title, string message, string informativeText) {
        return Task.Run(() => Show(title, message, informativeText));
    }
}