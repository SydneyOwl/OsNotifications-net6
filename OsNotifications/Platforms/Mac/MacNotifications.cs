using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OsNotifications.Platforms.Mac;

[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments")]
internal static class MacNotifications {
	[DllImport("macNotification.dylib")]
	private static extern void setGuiApplication(sbyte isGuiValue);

	[DllImport("macNotification.dylib")]
	private static extern void showNotification(
		[MarshalAs(UnmanagedType.LPUTF8Str)] string identifier,
		[MarshalAs(UnmanagedType.LPUTF8Str)] string title,
		[MarshalAs(UnmanagedType.LPUTF8Str)] string subtitle,
		[MarshalAs(UnmanagedType.LPUTF8Str)] string informativeText);

	public static void SetGuiApplication(bool isGuiValue) {
		setGuiApplication(isGuiValue ? (sbyte)1 : (sbyte)0);
	}

	public static void Show(string title, string message, string informativeText, string applicationIdentifier, bool isApplicationTypeSpecified) {
		if (!isApplicationTypeSpecified)
			throw new InvalidOperationException("SetGuiApplication must be called before calling ShowNotification. If SetGuiApplication is called with false in a GUI application, this method WILL HANG!");

		showNotification(applicationIdentifier, title, message, informativeText);
	}

	public static Task ShowAsync(string title, string message, string informativeText, string applicationIdentifier, bool isApplicationTypeSpecified) {
		return Task.Run(() => Show(title, message, informativeText, applicationIdentifier, isApplicationTypeSpecified));
	}
}
