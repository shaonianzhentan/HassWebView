namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class EntityRow : ContentView
{
    private bool _isInitialized = false;

    public EntityRow()
    {
        InitializeComponent();
        
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            try
            {
                UpdateColors();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EntityRow initialization failed: {ex}");
            }
        });
    }

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(EntityRow), string.Empty);

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EntityRow), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(EntityRow), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is EntityRow row)
                    row.SafeUpdateColors();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(EntityRow), false,
            propertyChanged: (b, _, __) =>
            {
                if (b is EntityRow row)
                    row.SafeUpdateColors();
            });

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public Color IconBackgroundColor
    {
        get => (Color)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(EntityRow), Colors.Transparent);

    public Color IconTextColor
    {
        get => (Color)GetValue(IconTextColorProperty);
        set => SetValue(IconTextColorProperty, value);
    }

    public static readonly BindableProperty IconTextColorProperty =
        BindableProperty.Create(nameof(IconTextColor), typeof(Color), typeof(EntityRow), Colors.Black);

    public Color StateColor
    {
        get => (Color)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(EntityRow), Colors.Gray);

    private void SafeUpdateColors()
    {
        if (_isInitialized)
        {
            UpdateColors();
        }
    }

    private void UpdateColors()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;

            var isDark = Application.Current.RequestedTheme == AppTheme.Dark;
            var isActive = IsActive || (State?.ToLower() == "on" || State?.ToLower() == "open");

            if (isActive)
            {
                IconBackgroundColor = (Color)resources["SuccessColor"];
                IconTextColor = Colors.White;
                StateColor = (Color)resources["SuccessColor"];
            }
            else
            {
                IconBackgroundColor = isDark ? Color.FromArgb("#3A3A3C") : Color.FromArgb("#EFEFF4");
                IconTextColor = isDark ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#1D1D1F");
                StateColor = isDark ? Color.FromArgb("#8E8E93") : Color.FromArgb("#636366");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EntityRow.UpdateColors failed: {ex}");
        }
    }
}