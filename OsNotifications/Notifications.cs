using System.Runtime.InteropServices;

namespace OsNotifications;

public partial class Notifications {
	public static string BundleIdentifier = "";
	public static string? WindowsApplicationId { get; set; }
	public static string? WindowsApplicationName { get; set; }

	private static bool _isApplicationTypeSpecified;
	private static Uri? _windowsAudioSource;
	private static bool _playDefaultWindowsSound = true;
	private static bool PlayDefaultWindowsSound => _playDefaultWindowsSound;

	static Notifications() {
		InitializeLinuxNotifications();
	}

	public static Uri? WindowsAudioSource {
		get => _windowsAudioSource;
		set {
			_windowsAudioSource = value;
			_playDefaultWindowsSound = false;
		}
	}

	public static void ResetWindowsAudioSource() => _playDefaultWindowsSound = true;

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
}
