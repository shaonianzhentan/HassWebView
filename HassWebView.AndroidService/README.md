# Android Foreground Service & Notification Library for .NET MAUI

A .NET MAUI library for easily creating and managing a foreground service and sending notifications on Android, with special features for Home Assistant.

## Features

- **Two Notification Types**: 
    - **Foreground Service**: A persistent, silent notification for long-running tasks.
    - **Standard Notifications**: Regular, dismissible notifications with sound/vibration.
- **Automatic & Manual ID**: Automatically generates unique IDs for new notifications, but allows manual ID assignment for updating specific ones.
- **Click Actions**: Notifications can either open the app or a specific URL.
- **Dependency Injection Ready**: Simple setup in `MauiProgram.cs`.
- **Lifecycle Control**: Easily start, stop, and update the foreground service.
- **Simplified Permission Handling**: A single method to handle all aspects of notification permissions (Android 13+).
- **Interactive Buttons**: Add custom action buttons to notifications.

## How to Use

### 1. Register the Service

In `MauiProgram.cs`, call the `UseForegroundService()` extension method.

```csharp
// In MauiProgram.cs
using HassWebView.AndroidService;

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
```

### 2. Inject the Service & Handle Actions

Use constructor injection to get an instance of `INotificationService`.

```csharp
private readonly INotificationService _notificationService;

public MainPage(INotificationService notificationService)
{
    InitializeComponent();
    _notificationService = notificationService;
    
    // Subscribe to button clicks from any notification
    _notificationService.ActionTriggered += OnNotificationAction;
}

private void OnNotificationAction(string actionId)
{
    MainThread.BeginInvokeOnMainThread(async () =>
    {
        await DisplayAlert("Action Triggered", $"Button '{actionId}' was clicked.", "OK");
    });
}
```

### 3. Ensure Notification Permission (Android 13+)

Before showing any notification, you must request permission.

```csharp
var hasPermission = await _notificationService.EnsurePermissionIsGrantedAsync();
if (!hasPermission) return; // The library will guide the user to settings
```

### 4. Showing a Standard Notification

These notifications use the default system sound and can be dismissed by the user.

#### Basic Usage (Auto-Generated ID)

The library automatically generates a unique, safe ID for each notification.

```csharp
// Show a notification that opens the app
int notificationId = await _notificationService.ShowNotificationAsync(
    "New Message",
    "You have a new message from a friend."
);

// Show a notification that opens a URL (e.g., from Home Assistant)
int urlNotificationId = await _notificationService.ShowNotificationAsync(
    "Security Alert",
    "Camera detected motion. Tap to view.",
    clickUrl: "https://your-home-assistant/lovelace/cameras"
);
```

#### Advanced: Manual ID for Updates

If you need to update a specific notification later, provide your own ID.

```csharp
const int orderStatusId = 5001;

// Show initial status
await _notificationService.ShowNotificationAsync(
    orderStatusId, 
    "Order Status", 
    "Your package has been shipped."
);

// ... later, update the same notification ...
await _notificationService.ShowNotificationAsync(
    orderStatusId, 
    "Order Status", 
    "Your package is out for delivery."
);
```

### 5. Foreground Service (Silent & Persistent)

Use this for long-running background tasks. The notification is silent and cannot be dismissed by the user.

```csharp
await _notificationService.StartAsync(
    "Service is Running", 
    "This is a silent, persistent notification."
);

// Stop the service when no longer needed
_notificationService.Stop();
```
