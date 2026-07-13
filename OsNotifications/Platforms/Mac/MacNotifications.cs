using System.Runtime.InteropServices;

namespace OsNotifications.Platforms.Mac;

/// <summary>
/// Maps to <c>UNAuthorizationStatus</c>.
/// </summary>
public enum MacNotificationPermissionStatus {
    NotDetermined = 0,
    Denied = 1,
    Authorized = 2,
    Provisional = 3,
    Ephemeral = 4,
    Error = -1
}

internal static class MacNotifications {
    // --- Callback delegate ---

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MacNotificationCallback(int result, IntPtr userData);

    // --- Async native imports ---

    [DllImport("macNotification.dylib")]
    private static extern void requestNotificationPermissionAsync(
        MacNotificationCallback callback, IntPtr userData);

    [DllImport("macNotification.dylib")]
    private static extern void getNotificationPermissionStatusAsync(
        MacNotificationCallback callback, IntPtr userData);

    [DllImport("macNotification.dylib")]
    private static extern void showNotificationAsync(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string title,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string subtitle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string body,
        MacNotificationCallback callback, IntPtr userData);

    // --- Sync native imports ---

    [DllImport("macNotification.dylib")]
    private static extern int getNotificationPermissionStatus();

    [DllImport("macNotification.dylib")]
    private static extern int requestNotificationPermission();

    [DllImport("macNotification.dylib")]
    private static extern int showNotification(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string title,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string subtitle,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string body);

    // --- Static callback delegates (pinned fields, never GC'd) ---

    private static readonly MacNotificationCallback _permissionCallback = OnPermissionDone;
    private static readonly MacNotificationCallback _statusCallback = OnStatusDone;
    private static readonly MacNotificationCallback _showCallback = OnShowDone;

    private static void OnPermissionDone(int result, IntPtr userData) {
        var handle = GCHandle.FromIntPtr(userData);
        var tcs = (TaskCompletionSource<bool>)handle.Target!;
        handle.Free();
        tcs.TrySetResult(result == 1);
    }

    private static void OnStatusDone(int result, IntPtr userData) {
        var handle = GCHandle.FromIntPtr(userData);
        var tcs = (TaskCompletionSource<int>)handle.Target!;
        handle.Free();
        tcs.TrySetResult(result);
    }

    private static void OnShowDone(int result, IntPtr userData) {
        var handle = GCHandle.FromIntPtr(userData);
        var tcs = (TaskCompletionSource<bool>)handle.Target!;
        handle.Free();
        tcs.TrySetResult(result == 0);
    }

    // --- Async API (per-call GCHandle, no global state) ---

    public static Task<bool> RequestPermissionAsync() {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(tcs);
        requestNotificationPermissionAsync(_permissionCallback, GCHandle.ToIntPtr(handle));
        return tcs.Task;
    }

    public static Task<MacNotificationPermissionStatus> GetPermissionStatusAsync() {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(tcs);
        getNotificationPermissionStatusAsync(_statusCallback, GCHandle.ToIntPtr(handle));
        return tcs.Task.ContinueWith(t =>
            t.Result >= 0 ? (MacNotificationPermissionStatus)t.Result : MacNotificationPermissionStatus.Error);
    }

    public static Task<bool> ShowAsync(string title, string message, string informativeText) {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = GCHandle.Alloc(tcs);
        showNotificationAsync(title, message, informativeText, _showCallback, GCHandle.ToIntPtr(handle));
        return tcs.Task;
    }

    // --- Sync API ---

    public static MacNotificationPermissionStatus GetPermissionStatus() {
        int raw = getNotificationPermissionStatus();
        return raw >= 0 ? (MacNotificationPermissionStatus)raw : MacNotificationPermissionStatus.Error;
    }

    public static bool IsPermissionGranted() {
        MacNotificationPermissionStatus status = GetPermissionStatus();
        return status is MacNotificationPermissionStatus.Authorized
                    or MacNotificationPermissionStatus.Provisional
                    or MacNotificationPermissionStatus.Ephemeral;
    }

    public static bool RequestPermission() {
        return requestNotificationPermission() == 1;
    }

    public static bool Show(string title, string message, string informativeText) {
        return showNotification(title, message, informativeText) == 0;
    }
}