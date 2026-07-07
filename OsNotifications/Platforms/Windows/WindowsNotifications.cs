using Microsoft.Toolkit.Uwp.Notifications;

namespace OsNotifications.Platforms.Windows;

// Modified from https://github.com/pr8x/DesktopNotifications
internal static class WindowsNotifications {
	private static bool _isWindowsApplicationRegistered;

	public static void Show(string title, string message, string applicationName, string applicationIdentifier) {
		EnsureApplicationRegistered(applicationName, applicationIdentifier);

		ToastContentBuilder toastContentBuilder = new ToastContentBuilder()
			.AddText(title)
			.AddText(message);

		toastContentBuilder.Show();
	}

	public static Task ShowAsync(string title, string message, string applicationName, string applicationIdentifier) {
		Show(title, message, applicationName, applicationIdentifier);
		return Task.CompletedTask;
	}

	private static void EnsureApplicationRegistered(string applicationName, string applicationIdentifier) {
		if (_isWindowsApplicationRegistered)
			return;

		WindowsApplicationRegistration.Register(applicationName, applicationIdentifier);
		_isWindowsApplicationRegistered = true;
	}
}
