# Native OS notifications

Modified from https://github.com/DemonExposer/OsNotifications. Downgraded deps to net6 and use a more "native" way to send notifications on windows.

## Usage
Calling `Notifications.ShowNotification` will display a notification using your OS's notification manager.

On MacOS, if you are working without a bundle and you do not want the notification to show Apple's "Finder", you need to specify the `BundleIdentifier` and this needs to correspond to a defined identifier, otherwise no notification will be shown. It is better to bundle your application to get your own application's icon to show up, but you could spoof another application as well (there is no guarantee that this works with every application though). Also, it must be specified whether the application is a console or GUI application, using `SetGuiApplication`, because if it is a GUI application and it is specified that it is a console application, the application would hang.
For cross-platform compatibility, it's better to always do this. So, creating a notification will look like this:
```cs
Notifications.BundleIdentifier = "com.apple.finder"; // Optional (does nothing for bundled applications)
Notifications.SetGuiApplication(true); // false for console application
try {
    Notifications.ShowNotification("notification-title");
} catch (PlatformNotSupportedException e) {
    // Handle exception
}
```

Of course this works without flaws on Linux and MacOS, but not on Windows... <br/>
If the application exits right after the `ShowNotification` call, the notification will not be shown on Windows, because of the asynchronous nature of Toast notifications. So, either don't display a notification just before exiting or just put a tiny delay (1000ms should be plenty) after the `ShowNotification` call.

This library uses `Microsoft.Toolkit.Uwp.Notifications` to display notifications on Windows. Windows support is compiled into the `net6.0-windows10.0.17763.0` target and registers the current process with an Application User Model ID before showing the toast. You can optionally set `Notifications.WindowsApplicationName` and `Notifications.WindowsApplicationId` before the first notification to control the Start Menu shortcut name and AUMID.

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
