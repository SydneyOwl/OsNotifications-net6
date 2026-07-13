using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OsNotifications.TestApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        BtnCheckStatus.Click += OnCheckStatus;
        BtnRequestPermission.Click += OnRequestPermission;
        BtnShowNotification.Click += OnShowNotification;
        BtnShowCustom.Click += OnShowCustom;
    }

    private void OnCheckStatus(object? sender, RoutedEventArgs e)
    {
        var status = Notifications.GetNotificationPermissionStatus();
        TxtStatus.Text = $"Status: {status}";
    }

    private async void OnRequestPermission(object? sender, RoutedEventArgs e)
    {
        BtnRequestPermission.IsEnabled = false;
        TxtPermissionResult.Text = "Requesting...";

        var granted = await Notifications.RequestNotificationPermissionAsync();

        TxtPermissionResult.Text = granted ? "Granted!" : "Denied or error";
        BtnRequestPermission.IsEnabled = true;
    }

    private async void OnShowNotification(object? sender, RoutedEventArgs e)
    {
        BtnShowNotification.IsEnabled = false;
        TxtShowResult.Text = "Sending...";

        var ok = await Notifications.ShowNotificationAsync(
            "Hello from OsNotifications",
            "Test App",
            "This is a test notification.");

        TxtShowResult.Text = ok ? "Sent OK" : "Failed";
        BtnShowNotification.IsEnabled = true;
    }

    private async void OnShowCustom(object? sender, RoutedEventArgs e)
    {
        var title = string.IsNullOrWhiteSpace(TxtTitle.Text) ? "Test" : TxtTitle.Text;
        var body = string.IsNullOrWhiteSpace(TxtBody.Text) ? "Custom message" : TxtBody.Text;

        BtnShowCustom.IsEnabled = false;

        var ok = await Notifications.ShowNotificationAsync(title, body);

        BtnShowCustom.IsEnabled = true;
    }
}