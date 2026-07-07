using Microsoft.Toolkit.Uwp.Notifications;

namespace OsNotifications;

public partial class Notifications {
	private static bool _isWindowsApplicationRegistered;

	private static void ShowNotificationWindows(string title, string message) {
		EnsureWindowsApplicationRegistered();

		ToastContentBuilder toastContentBuilder = new ToastContentBuilder()
			.AddText(title)
			.AddText(message);

		toastContentBuilder.Show();
	}

	private static void EnsureWindowsApplicationRegistered() {
		if (_isWindowsApplicationRegistered)
			return;

		WindowsApplicationRegistration.Register(GetApplicationName(), GetApplicationIdentifier());
		_isWindowsApplicationRegistered = true;
	}
}
