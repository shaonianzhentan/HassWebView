using System.Windows.Input;
using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class ButtonCard : ContentView
{
    public ButtonCard()
    {
        InitializeComponent();
        UpdateActiveAppearance();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is not null)
        {
            if (Application.Current is not null)
                Application.Current.RequestedThemeChanged += OnThemeChanged;
        }
        else
        {
            if (Application.Current is not null)
                Application.Current.RequestedThemeChanged -= OnThemeChanged;
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateActiveAppearance();
    }

    /// <summary>
    /// Fired when the card is tapped.
    /// </summary>
    public event EventHandler? Tapped;

    #region Bindable Properties

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(ButtonCard), null,
            propertyChanged: (b, _, _) => ((ButtonCard)b).UpdateIconLabel());

    /// <summary>
    /// Text or emoji character shown inside the icon shape.
    /// </summary>
    public string? IconText
    {
        get => (string?)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(ButtonCard), null,
            propertyChanged: (b, _, _) => ((ButtonCard)b).UpdateIconLabel());

    /// <summary>
    /// ImageSource shown inside the icon shape (takes priority over IconText).
    /// </summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ButtonCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(ButtonCard), null,
            propertyChanged: (b, _, _) => ((ButtonCard)b).UpdateActiveAppearance());

    /// <summary>
    /// State text displayed below the title. Also drives StateLabel color.
    /// </summary>
    public string? State
    {
        get => (string?)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(ButtonCard), false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (b, _, _) => ((ButtonCard)b).UpdateActiveAppearance());

    /// <summary>
    /// Controls the icon background tint and state label color.
    /// true → orange (On), false → neutral gray (Off).
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly BindableProperty ShowBadgeProperty =
        BindableProperty.Create(nameof(ShowBadge), typeof(bool), typeof(ButtonCard), false,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is ButtonCard card)
                    card.Badge.IsVisible = (bool)newValue;
            });

    /// <summary>
    /// When true, shows the StateBadge in the top-right corner.
    /// The badge State follows this card's State property.
    /// </summary>
    public bool ShowBadge
    {
        get => (bool)GetValue(ShowBadgeProperty);
        set => SetValue(ShowBadgeProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ButtonCard), null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ButtonCard), null);

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(ButtonCard), ComponentSize.Medium);

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    #endregion

    private void UpdateIconLabel()
    {
        if (Icon is not null)
        {
            IconImage.Source = Icon;
            IconImage.IsVisible = true;
            IconLabel.IsVisible = false;
        }
        else
        {
            IconLabel.Text = IconText;
            IconLabel.IsVisible = true;
            IconImage.IsVisible = false;
        }
    }

    private void UpdateActiveAppearance()
    {
        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        string? stateLower = State?.ToLowerInvariant();

        // Sync badge state
        Badge.State = State ?? "off";

        if (IsActive || stateLower == "on")
        {
            // Warm orange tint icon background
            IconBorder.BackgroundColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#FFA726").WithAlpha(0.20f)
                : Microsoft.Maui.Graphics.Color.FromArgb("#FF9800").WithAlpha(0.15f);

            // Orange state label
            StateLabel.TextColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#FFA726")
                : Microsoft.Maui.Graphics.Color.FromArgb("#FF9800");
        }
        else if (stateLower is "unavailable" or "unknown")
        {
            // Red tint icon background
            IconBorder.BackgroundColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#EF5350").WithAlpha(0.20f)
                : Microsoft.Maui.Graphics.Color.FromArgb("#F44336").WithAlpha(0.15f);

            // Red state label
            StateLabel.TextColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#EF5350")
                : Microsoft.Maui.Graphics.Color.FromArgb("#F44336");
        }
        else
        {
            // Neutral gray icon background
            IconBorder.BackgroundColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#3A3A3C")
                : Microsoft.Maui.Graphics.Color.FromArgb("#EBEBEB");

            // Gray state label
            StateLabel.TextColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#8E8E93")
                : Microsoft.Maui.Graphics.Color.FromArgb("#9E9E9E");
        }
    }

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        // HA 2026-style elastic tap feedback
        await this.ScaleTo(0.96, 50, Easing.CubicOut);
        await this.ScaleTo(1.0, 80, Easing.SpringOut);

        Tapped?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) ?? false)
            Command.Execute(CommandParameter);
    }
}
