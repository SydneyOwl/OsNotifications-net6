namespace OsNotifications;

public partial class Notifications {
	private static void ShowNotificationWindows(string title, string message) {
		throw new PlatformNotSupportedException("Windows notifications require a Windows target framework, for example net6.0-windows10.0.17763.0.");
	}
}
