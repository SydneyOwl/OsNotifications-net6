# Native OS notifications

Modified from https://github.com/DemonExposer/OsNotifications. Downgraded deps to net6 and use a more "native" way to send notifications on windows.

## Usage
Call `Notifications.ShowNotification` to display a notification through the current OS notification manager.

```csharp
using OsNotifications;

try {
    Notifications.ShowNotification(
        "Notification title",
        "Notification message");
} catch (PlatformNotSupportedException) {
    // Handle unsupported platforms.
}
```

### Windows

For Windows toast notifications, target a Windows TFM:

```xml
<TargetFramework>net6.0-windows10.0.17763.0</TargetFramework>
```

The library registers an Application User Model ID (AUMID) before showing the first toast. If you do not set one, it uses the current executable name. For production apps, set a stable name and AUMID before the first notification:

```csharp
using OsNotifications;

Notifications.WindowsApplicationName = "My App";
Notifications.WindowsApplicationId = "com.example.myapp";

Notifications.ShowNotification("Hello", "This toast is associated with My App.");
```

The Windows implementation also creates a Start Menu shortcut with the same AUMID so unpackaged desktop apps are correctly recognized by Windows Shell.

If the application exits immediately after `ShowNotification`, Windows may not display the toast because toast delivery is asynchronous. Keep the process alive briefly when sending a notification during shutdown.

### macOS

On macOS, call `SetGuiApplication` before showing notifications:

```csharp
using OsNotifications;

Notifications.BundleIdentifier = "com.apple.finder"; // Optional for bundled apps.
Notifications.SetGuiApplication(true);               // false for console apps.

Notifications.ShowNotification(
    "Notification title",
    "Subtitle",
    "Informative text");
```

If you are not running from an app bundle, `BundleIdentifier` must match an installed bundle identifier. Otherwise macOS may ignore the notification or show it under another application.

### Linux

Linux notifications use the FreeDesktop notification service over DBus:

```csharp
using OsNotifications;

Notifications.ShowNotification("Build complete", "No errors found.");
```

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
