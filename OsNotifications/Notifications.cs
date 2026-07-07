using System.Runtime.InteropServices;
using System.Reflection;
using OsNotifications.Platforms.Linux;
using OsNotifications.Platforms.Mac;
using OsNotifications.Platforms.Windows;

namespace OsNotifications;

public partial class Notifications {
	public static string? ApplicationIdentifier { get; set; }
	public static string? ApplicationName { get; set; }

	private static bool _isApplicationTypeSpecified;

	static Notifications() {
		LinuxNotifications.Initialize();
	}

	public static void SetGuiApplication(bool isGuiValue) {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return;

		MacNotifications.SetGuiApplication(isGuiValue);
		_isApplicationTypeSpecified = true;
	}

	public static void ShowNotification(string title, string message = "", string informativeText = "") {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			LinuxNotifications.Show(title, message, GetApplicationName());
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			MacNotifications.Show(title, message, informativeText, ApplicationIdentifier ?? "", _isApplicationTypeSpecified);
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			WindowsNotifications.Show(title, message, GetApplicationName(), GetApplicationIdentifier());
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
