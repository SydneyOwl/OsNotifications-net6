using System.Runtime.InteropServices;

namespace OsNotifications;

public partial class Notifications {
	[DllImport("macNotification.dylib")]
	private static extern void setGuiApplication(sbyte isGuiValue);

	[DllImport("macNotification.dylib")]
	private static extern void showNotification(
		[MarshalAs(UnmanagedType.LPStr)] string identifier,
		[MarshalAs(UnmanagedType.LPStr)] string title,
		[MarshalAs(UnmanagedType.LPStr)] string subtitle,
		[MarshalAs(UnmanagedType.LPStr)] string informativeText);

	private static void SetGuiApplicationMac(bool isGuiValue) {
		setGuiApplication(isGuiValue ? (sbyte)1 : (sbyte)0);
	}

	private static void ShowNotificationMac(string title, string message, string informativeText) {
		if (!_isApplicationTypeSpecified)
			throw new InvalidOperationException("SetGuiApplication must be called before calling ShowNotification. If SetGuiApplication is called with false in a GUI application, this method WILL HANG!");

		showNotification(ApplicationIdentifier ?? "", title, message, informativeText);
	}
}
