using System.Runtime.InteropServices;

namespace OsNotifications.Platforms.Mac;

internal static class MacNotifications {
	[DllImport("macNotification.dylib")]
	private static extern void setGuiApplication(sbyte isGuiValue);

	[DllImport("macNotification.dylib")]
	private static extern void showNotification(
		[MarshalAs(UnmanagedType.LPStr)] string identifier,
		[MarshalAs(UnmanagedType.LPStr)] string title,
		[MarshalAs(UnmanagedType.LPStr)] string subtitle,
		[MarshalAs(UnmanagedType.LPStr)] string informativeText);

	public static void SetGuiApplication(bool isGuiValue) {
		setGuiApplication(isGuiValue ? (sbyte)1 : (sbyte)0);
	}

	public static void Show(string title, string message, string informativeText, string applicationIdentifier, bool isApplicationTypeSpecified) {
		if (!isApplicationTypeSpecified)
			throw new InvalidOperationException("SetGuiApplication must be called before calling ShowNotification. If SetGuiApplication is called with false in a GUI application, this method WILL HANG!");

		showNotification(applicationIdentifier, title, message, informativeText);
	}
}
