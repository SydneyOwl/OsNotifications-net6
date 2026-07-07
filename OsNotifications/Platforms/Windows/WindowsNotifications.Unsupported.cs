namespace OsNotifications.Platforms.Windows;

internal static class WindowsNotifications {
	public static void Show(string title, string message, string applicationName, string applicationIdentifier) {
		throw new PlatformNotSupportedException("Windows notifications require a Windows target framework, for example net6.0-windows10.0.17763.0.");
	}
}
