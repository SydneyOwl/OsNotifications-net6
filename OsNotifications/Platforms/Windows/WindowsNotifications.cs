using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace OsNotifications.Platforms.Windows;

// Modified from https://github.com/pr8x/DesktopNotifications
internal static class WindowsNotifications {
	private static bool _isInitialized;
	private static string? _registeredAumid;

	public static void Initialize(string applicationName, string applicationIdentifier) {
		WindowsApplicationRegistration.Register(applicationName, applicationIdentifier);

		_registeredAumid = string.IsNullOrWhiteSpace(applicationIdentifier)
			? applicationName?.Trim() ?? ""
			: applicationIdentifier.Trim();

		_isInitialized = true;
	}

	public static void Show(string title, string message) {
		ToastContentBuilder toastContentBuilder = new ToastContentBuilder()
			.AddText(title)
			.AddText(message);

		toastContentBuilder.Show();
	}

	public static Task ShowAsync(string title, string message) {
		Show(title, message);
		return Task.CompletedTask;
	}

	public static bool IsPermissionGranted() {
		if (!_isInitialized || _registeredAumid == null)
			return true;

		using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
			$@"SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings\{_registeredAumid}");

		if (key == null)
			return true;

		object? enabled = key.GetValue("Enabled");
		return enabled is not int value || value != 0;
	}
}