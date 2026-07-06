using System.Reflection;
using System.Runtime.InteropServices;
using Tmds.DBus;

namespace OsNotifications;

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

public partial class Notifications {
	private static INotifier? _notifier;

	private static void InitializeLinuxNotifications() {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return;

		Connection? connection = Connection.Session;
		connection.ConnectAsync().GetAwaiter().GetResult();
		_notifier = connection.CreateProxy<INotifier>(
			"org.freedesktop.Notifications",
			"/org/freedesktop/Notifications");

		AppDomain.CurrentDomain.ProcessExit += (_, _) => connection.Dispose();
	}

	private static void ShowNotificationLinux(string title, string message) {
		_notifier!.NotifyAsync(Assembly.GetEntryAssembly()?.GetName().Name ?? "", 0, "", title, message,
			Array.Empty<string>(), new Dictionary<string, object>(), 5000).GetAwaiter().GetResult();
	}
}
