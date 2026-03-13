# HassWebView.Component

A library of custom UI components for .NET MAUI.

## Getting Started

To use the components in this library, you need to reference the `HassWebView.Component` project and initialize it in your `MauiProgram.cs`.

### 1. Initialization

In your `MauiProgram.cs`, chain the `UseHassComponents()` method to your `MauiAppBuilder`:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseHassComponents() // <-- Add this line
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
```

This will automatically load the required styles for the components, including support for light and dark modes.

## Components

### SwitchCard

`SwitchCard` is a versatile card component designed for feature toggles. It includes a title, an optional description, and a switch.

![SwitchCard](https://i.imgur.com/example.png) // Placeholder - You can replace this with an actual screenshot

### How to Use

1.  **Add the XML Namespace**

    In your XAML file, add a namespace declaration for the component library:

    ```xml
    xmlns:hass="clr-namespace:HassWebView.Component;assembly=HassWebView.Component"
    ```

2.  **Add the Component to Your Page**

    You can now use the `SwitchCard` in your layout.

    **XAML Example:**

    ```xml
    <VerticalStackLayout Spacing="15" Padding="20">

        <!-- Card with Title, Description, and data binding -->
        <hass:SwitchCard 
            Title="Enable Advanced Analytics"
            Description="Unlock detailed usage statistics and insights."
            IsToggled="{Binding IsAnalyticsEnabled, Mode=TwoWay}"
            Toggled="OnAnalyticsToggled" />

        <!-- A simpler card without a description -->
        <hass:SwitchCard 
            Title="Dark Mode"
            IsToggled="{Binding IsDarkMode, Mode=TwoWay}" />

    </VerticalStackLayout>
    ```

### Properties

*   `Title` (string): The main title of the card.
*   `Description` (string): Optional text that appears below the title. If left empty or `null`, it will not be displayed.
*   `IsToggled` (bool): The on/off state of the switch. This is a bindable property and supports `TwoWay` binding.

### Events

*   `Toggled`: This event is fired whenever the user flips the switch. The event arguments `ToggledEventArgs` contain the new `bool` value.

    **Event Handler Example:**

    ```csharp
    private void OnAnalyticsToggled(object sender, ToggledEventArgs e)
    {
        bool isNowEnabled = e.Value;
        Console.WriteLine($"Advanced Analytics is now: {(isNowEnabled ? "Enabled" : "Disabled")}");
        // You can now update your settings or application state
    }
    ```
