# Native OS notifications

Modified from https://github.com/DemonExposer/OsNotifications. Downgraded deps to net6 and use a more "native" way to send notifications on windows.

## Usage

```csharp
using OsNotifications;

// Set application identity (required for Windows, used as display name on Linux)
Notifications.SetApplicationName("My App");
Notifications.SetApplicationIdentifier("com.example.myapp");

// macOS: request permission once at startup; does nothing on windows and linux.
Notifications.RequestNotificationPermission();

try {
    Notifications.ShowNotification(
        "Notification title",
        "Notification message");
} catch (PlatformNotSupportedException) {
    // Handle unsupported platforms.
}
```

Async usage is also available:

```csharp
await Notifications.ShowNotificationAsync(
    "Notification title",
    "Notification message");
```

### Windows

For Windows toast notifications, target a Windows TFM:

```xml
<TargetFramework>net6.0-windows10.0.17763.0</TargetFramework>
```

The Windows implementation creates a Start Menu shortcut with the same AUMID so unpackaged desktop apps are correctly recognized by Windows Shell, so we need to register them before showing the first toast. 
call `SetApplicationIdentifier` sets the AUMID, and `SetApplicationName` sets the Start Menu shortcut name.

If you do not call them, the current executable name is used for both.

```csharp
using OsNotifications;

Notifications.SetApplicationName("My App");
Notifications.SetApplicationIdentifier("com.example.myapp");

Notifications.ShowNotification("Hello", "This toast is associated with My App.");
```

If the application exits immediately after `ShowNotification`, Windows may not display the toast because toast delivery is asynchronous. Keep the process alive briefly when sending a notification during shutdown.

### macOS

Your app must be running from a proper macOS bundle.

- (optional) You can always call `RequestNotificationPermission` before scheduling any notifications. The system prompts the user only on the very first call, 
and stores the response permanently. Subsequent calls (even across app restarts) return immediately without prompting.
- The user may change authorization at any time in system settings. You can safely call this method repeatedly — it will never show the prompt again after the first time.

```csharp
using OsNotifications;

Notifications.RequestNotificationPermission();

// Then show notifications anywhere in your app
Notifications.ShowNotification(
    "Notification title",
    "Subtitle",
    "Informative text");
```

If the user denies authorization, notifications will be silently ignored by the system.

### Linux

Linux notifications use the FreeDesktop notification service over DBus:

```csharp
using OsNotifications;

Notifications.ShowNotification("Build complete", "No errors found.");
```

`SetApplicationName` sets the FreeDesktop application name. If not called, the current executable name is used.

## Install
In your .NET project, execute the following command:
```
dotnet add package OsNotificationsNet6
```

## Build from source
The best way to build this project is on MacOS, because of the Objective-C code needing to be compiled.
Make sure you have `clang` installed and then just run the following commands in the `OsNotifications` folder (so the folder that contains `OsNotifications.csproj`):
```
dotnet build --configuration Release
dotnet pack --no-build --configuration Release
```
You will get a `.nupkg` file which you can import using NuGet.
Probably, just running `dotnet build` or `dotnet publish ...` while referencing the project will not work properly. I'm not sure about it, but I recommend using the `pack` command.

It should be possible to build this on Linux as well, you just need to find the appropriate libraries to compile Objective-C with and then probably change the `clang` command in `OsNotifications/OsNotifications.csproj` to something more Linux-friendly.
