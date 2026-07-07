using System.Runtime.InteropServices;
using Tmds.DBus;

namespace OsNotifications.Platforms.Linux;

[DBusInterface("org.freedesktop.Notifications")]
public interface INotifier : IDBusObject {
	Task<uint> NotifyAsync(
		string appName,
		uint replacesId,
		string appIcon,
		string summary,
		string body,
		string[] actions,
		IDictionary<string, object> hints,
		int expireTimeout);
}

internal static class LinuxNotifications {
	private static INotifier? _notifier;

	public static void Initialize() {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return;

		Connection? connection = Connection.Session;
		connection.ConnectAsync().GetAwaiter().GetResult();
		_notifier = connection.CreateProxy<INotifier>(
			"org.freedesktop.Notifications",
			"/org/freedesktop/Notifications");

		AppDomain.CurrentDomain.ProcessExit += (_, _) => connection.Dispose();
	}

	public static void Show(string title, string message, string applicationName) {
		_notifier!.NotifyAsync(applicationName, 0, "", title, message,
			Array.Empty<string>(), new Dictionary<string, object>(), 5000).GetAwaiter().GetResult();
	}
}
