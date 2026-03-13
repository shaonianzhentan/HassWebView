# How to Enable GKD-Based Ad Skipping

This guide explains how to integrate the self-contained, GKD-based ad skipping feature into your Android application. The `HassWebView.AndroidService` project is a fully independent module.

### 1. Project Reference

First, ensure your main application project (e.g., `HassWebView.Demo`) has a project reference to `HassWebView.AndroidService`.

### 2. Enable the Service in `MauiProgram.cs`

The entire feature can be activated with a single line of code in your `MauiProgram.cs` file.

In the `CreateMauiApp` method, add the `UseAdSkipping` call to the `MauiAppBuilder` chain. You must provide a URL to a `gkd` compatible rule subscription file.

```csharp
using HassWebView.AndroidService.AdSkipping; // Import the correct namespace

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // Add this line to enable ad skipping
            .UseAdSkipping("https://gkd-subscription-667.pages.dev/gkd.json5") 
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ... other services

        return builder.Build();
    }
}
```

### 3. Add Service Configuration to `AndroidManifest.xml`

You must declare the `AdSkippingService` in your `AndroidManifest.xml` file, located at `[YourAppProject]/Platforms/Android/AndroidManifest.xml`.

Add the following `<service>` block inside the `<application>` tag. This makes the Android system aware of your accessibility service.

```xml
<application ...>
    ...
    <service
        android:name="HassWebView.AndroidService.Platforms.Android.AdSkippingService"
        android:permission="android.permission.BIND_ACCESSIBILITY_SERVICE"
        android:label="My App Ad Skipping Service"
        android:exported="false">
        <intent-filter>
            <action android:name="android.accessibilityservice.AccessibilityService" />
        </intent-filter>
        <meta-data
            android:name="android.accessibilityservice"
            android:resource="@xml/accessibility_service_config" />
    </service>
</application>
```

### 4. Create the Accessibility Service Config File

Create a new XML file named `accessibility_service_config.xml` inside `[YourAppProject]/Platforms/Android/Resources/xml/`.

This file tells the Android system what your service needs to do. Paste the following content into it:

```xml
<?xml version="1.0" encoding="utf-8"?>
<accessibility-service xmlns:android="http://schemas.android.com/apk/res/android"
    android:description="@string/accessibility_service_description"
    android:packageNames=""
    android:accessibilityEventTypes="typeAllMask"
    android:accessibilityFlags="flagDefault|flagRetrieveWindowContent"
    android:accessibilityFeedbackType="feedbackGeneric"
    android:notificationTimeout="100"
    android:canRetrieveWindowContent="true" />
```

**Note:** You also need to define `accessibility_service_description` in your strings file (`Resources/values/strings.xml`):

```xml
<resources>
    <string name="accessibility_service_description">This service helps skip ads automatically.</string>
</resources>
```

### 5. Guide User to Enable the Service

The service will only run if the user explicitly enables it in their device's Accessibility settings.

You should provide a button or a link in your app's settings page to take the user directly there.

```csharp
var intent = new Android.Content.Intent(Android.Provider.Settings.ActionAccessibilitySettings);
intent.AddFlags(Android.Content.ActivityFlags.NewTask);
Android.App.Application.Context.StartActivity(intent);
```

Once the user enables the service, it will automatically download the rules and start skipping ads based on the GKD subscription.
