using System.Runtime.InteropServices;
using System.Reflection;

namespace OsNotifications;

public partial class Notifications {
	public static string? ApplicationIdentifier { get; set; }
	public static string? ApplicationName { get; set; }

	private static bool _isApplicationTypeSpecified;

	static Notifications() {
		InitializeLinuxNotifications();
	}

	public static void SetGuiApplication(bool isGuiValue) {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return;

		SetGuiApplicationMac(isGuiValue);
		_isApplicationTypeSpecified = true;
	}

	public static void ShowNotification(string title, string message = "", string informativeText = "") {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			ShowNotificationLinux(title, message);
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			ShowNotificationMac(title, message, informativeText);
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			ShowNotificationWindows(title, message);
		else
			throw new PlatformNotSupportedException("Notifications are only supported on Linux, MacOS and Windows");
	}

	private static string GetApplicationName() {
		if (!string.IsNullOrWhiteSpace(ApplicationName))
			return ApplicationName.Trim();

		return Assembly.GetEntryAssembly()?.GetName().Name ?? "";
	}

	private static string GetApplicationIdentifier() {
		if (!string.IsNullOrWhiteSpace(ApplicationIdentifier))
			return ApplicationIdentifier.Trim();

		return GetApplicationName();
	}
}
