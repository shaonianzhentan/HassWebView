using System.Windows.Input;
using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class EntityRow : ContentView
{
    public EntityRow()
    {
        InitializeComponent();
        UpdateIconAppearance();
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
        UpdateIconAppearance();
    }

    /// <summary>
    /// Fired when the row is tapped.
    /// </summary>
    public event EventHandler? Tapped;

    #region Bindable Properties

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(EntityRow), null,
            propertyChanged: (b, _, _) => ((EntityRow)b).UpdateIconLabel());

    /// <summary>
    /// Text or emoji character to display inside the icon shape.
    /// </summary>
    public string? IconText
    {
        get => (string?)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(EntityRow), null,
            propertyChanged: (b, _, _) => ((EntityRow)b).UpdateIconLabel());

    /// <summary>
    /// ImageSource to display inside the icon shape (takes priority over IconText).
    /// </summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EntityRow), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(EntityRow), null);

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(EntityRow), null,
            propertyChanged: (b, _, _) => ((EntityRow)b).UpdateIconAppearance());

    /// <summary>
    /// The entity state string (e.g. "on", "off", "unavailable"). Drives icon background color and opacity.
    /// </summary>
    public string? State
    {
        get => (string?)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(EntityRow), false,
            propertyChanged: (b, _, _) => ((EntityRow)b).UpdateIconAppearance());

    /// <summary>
    /// When true, uses the On (orange) icon background; otherwise uses neutral gray.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly BindableProperty RightContentProperty =
        BindableProperty.Create(nameof(RightContent), typeof(View), typeof(EntityRow), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is EntityRow row)
                {
                    if (newValue is View view)
                    {
                        row.RightPresenter.Content = view;
                    }
                    else
                    {
                        // Restore the default state label
                        row.RightPresenter.Content = row.DefaultStateLabel;
                    }
                }
            });

    /// <summary>
    /// Replace the right-side content. Defaults to a state text label.
    /// </summary>
    public View? RightContent
    {
        get => (View?)GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EntityRow), null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(EntityRow), null);

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(EntityRow), ComponentSize.Medium);

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

    private void UpdateIconAppearance()
    {
        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        string? stateLower = State?.ToLowerInvariant();

        // Whole-row opacity for unavailable
        Opacity = stateLower is "unavailable" or "unknown" ? 0.5 : 1.0;

        // Icon background color (semi-transparent)
        if (IsActive || stateLower == "on")
        {
            // Warm orange tint
            IconBorder.BackgroundColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#FFA726").WithAlpha(0.20f)
                : Microsoft.Maui.Graphics.Color.FromArgb("#FF9800").WithAlpha(0.15f);
        }
        else if (stateLower is "unavailable" or "unknown")
        {
            // Red tint
            IconBorder.BackgroundColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#EF5350").WithAlpha(0.20f)
                : Microsoft.Maui.Graphics.Color.FromArgb("#F44336").WithAlpha(0.15f);
        }
        else
        {
            // Neutral gray
            IconBorder.BackgroundColor = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#3A3A3C")
                : Microsoft.Maui.Graphics.Color.FromArgb("#EBEBEB");
        }

        // State label text color
        DefaultStateLabel.TextColor = stateLower switch
        {
            "on" => isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#FFA726")
                : Microsoft.Maui.Graphics.Color.FromArgb("#FF9800"),
            "unavailable" or "unknown" => isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#EF5350")
                : Microsoft.Maui.Graphics.Color.FromArgb("#F44336"),
            _ => isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#8E8E93")
                : Microsoft.Maui.Graphics.Color.FromArgb("#9E9E9E")
        };
    }

    private void OnTapped(object sender, TappedEventArgs e)
    {
        Tapped?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) ?? false)
            Command.Execute(CommandParameter);
    }
}
