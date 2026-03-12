# Android Foreground Service for .NET MAUI

A .NET MAUI library for easily creating and managing a foreground service on Android. This service displays a persistent notification, ideal for applications that need to perform long-running tasks in the background.

## Features

- **Dependency Injection Ready**: Simple setup in `MauiProgram.cs` using an extension method.
- **Start, Stop, and Update**: Easily control the lifecycle of the foreground service.
- **Simplified Permission Handling**: A single method handles checking, requesting, and guiding users to settings for notification permissions (Android 13+).
- **Interactive Buttons**: Add custom action buttons to the notification and receive events.
- **Fluent API**: A simple and intuitive C# interface (`INotificationService`).

## How to Use

### 1. Register the Service

In your `MauiProgram.cs`, call the `UseForegroundService()` extension method on the `MauiAppBuilder`.

```csharp
using HassWebView.AndroidService;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseForegroundService() // <-- Add this line
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
```

### 2. Inject and Use the Service

With the service registered, you can now inject `INotificationService` into your views, view models, or other services using constructor injection.

```csharp
// Example in a ContentPage
private readonly INotificationService _notificationService;

public MainPage(INotificationService notificationService)
{
    InitializeComponent();
    _notificationService = notificationService;
}
```

### 3. Ensure Notification Permission (Android 13+)

Before starting the service, ensure your app has the required permission. The library provides a single method to handle the entire process, including guiding the user to settings if needed.

```csharp
private async void OnStartServiceClicked(object sender, EventArgs e)
{
    // Use the injected service instance
    var hasPermission = await _notificationService.EnsurePermissionIsGrantedAsync("Notification permission is needed to run the service.");
    
    if (!hasPermission) {
        // The library has already guided the user to settings. 
        // You can optionally show another message or log this event.
        return;
    }
    
    // Permission granted, now you can start the service.
    await _notificationService.StartAsync("Service is Running", "This is a persistent notification.");
}

```

### 4. Start with Interactive Buttons

You can add buttons to the notification and handle their click events.

```csharp
private async void OnStartWithActionsClicked(object sender, EventArgs e)
{
    // Subscribe to the action event
    _notificationService.ActionTriggered += (actionId) =>
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (actionId == "stop-action")
            {
                _notificationService.Stop();
                await DisplayAlert("Service", "Service stopped by user.", "OK");
            }
            else
            { 
                await DisplayAlert("Action Triggered", $"Button '{actionId}' clicked.", "OK");
            }
        });
    };

    var actions = new List<NotificationAction> { new("stop-action", "Stop Service") };

    if (await _notificationService.EnsurePermissionIsGrantedAsync())
    {
        await _notificationService.StartAsync("Service with Actions", "Click a button!", actions: actions);
    }
}
```

### 5. Update and Stop

Use the same injected service instance to update or stop the service.

```csharp
private async void OnUpdateServiceClicked(object sender, EventArgs e)
{
    await _notificationService.UpdateAsync("Status Updated", "The service is now doing something new.");
}

private void OnStopServiceClicked(object sender, EventArgs e)
{
    _notificationService.Stop();
}
```
