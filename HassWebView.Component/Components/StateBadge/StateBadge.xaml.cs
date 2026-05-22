using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class StateBadge : Border
{
    public StateBadge()
    {
        InitializeComponent();
        UpdateAppearance();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is not null)
        {
            // Subscribe when attached to the visual tree
            if (Application.Current is not null)
                Application.Current.RequestedThemeChanged += OnThemeChanged;
        }
        else
        {
            // Unsubscribe when detached
            if (Application.Current is not null)
                Application.Current.RequestedThemeChanged -= OnThemeChanged;
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateAppearance();
    }

    #region Bindable Properties

    /// <summary>
    /// The state string driving the badge color. e.g. "on", "off", "unavailable", "unknown"
    /// </summary>
    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(StateBadge), "off",
            propertyChanged: (b, _, _) => ((StateBadge)b).UpdateAppearance());

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary>
    /// Controls the badge size.
    /// </summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(StateBadge), ComponentSize.Medium,
            propertyChanged: (b, _, _) => ((StateBadge)b).UpdateAppearance());

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Manually override the badge color. If set, ignores State-driven color.
    /// </summary>
    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(StateBadge), null,
            propertyChanged: (b, _, _) => ((StateBadge)b).UpdateAppearance());

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    #endregion

    private void UpdateAppearance()
    {
        // Update size
        double size = Size switch
        {
            ComponentSize.Small => 8,
            ComponentSize.Large => 16,
            _ => 12
        };
        WidthRequest = size;
        HeightRequest = size;

        // If Color is manually overridden, use it directly
        if (Color is not null)
        {
            BackgroundColor = Color;
            return;
        }

        // Determine dark mode
        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        // Map state to HA 2026 color
        BackgroundColor = State?.ToLowerInvariant() switch
        {
            "on" => isDark ? Microsoft.Maui.Graphics.Color.FromArgb("#FFA726")
                           : Microsoft.Maui.Graphics.Color.FromArgb("#FF9800"),
            "unavailable" or "unknown" =>
                isDark ? Microsoft.Maui.Graphics.Color.FromArgb("#EF5350")
                       : Microsoft.Maui.Graphics.Color.FromArgb("#F44336"),
            "off" => isDark ? Microsoft.Maui.Graphics.Color.FromArgb("#757575")
                            : Microsoft.Maui.Graphics.Color.FromArgb("#9E9E9E"),
            _ => isDark ? Microsoft.Maui.Graphics.Color.FromArgb("#03A9F4")
                        : Microsoft.Maui.Graphics.Color.FromArgb("#039BE5")
        };
    }
}
