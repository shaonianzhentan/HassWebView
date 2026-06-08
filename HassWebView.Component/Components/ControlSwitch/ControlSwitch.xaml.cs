namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;
using HassWebView.Component.Models;

public partial class ControlSwitch : AdaptiveComponent
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(ControlSwitch), false,
            propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty DisabledProperty =
        BindableProperty.Create(nameof(Disabled), typeof(bool), typeof(ControlSwitch), false);

    private bool _isInitialized;

    public ControlSwitch()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeManager.SizeChanged += OnGlobalSizeChanged;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            UpdateThumbPosition(false);
            _isInitialized = true;
        });
        Loaded -= OnLoaded;
    }

    private void OnGlobalSizeChanged(object? sender, EventArgs e)
    {
        if (_isInitialized)
        {
            UpdateThumbPosition(false);
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(EffectiveSize) && _isInitialized)
        {
            UpdateThumbPosition(false);
        }
    }

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public bool Disabled
    {
        get => (bool)GetValue(DisabledProperty);
        set => SetValue(DisabledProperty, value);
    }

    public event EventHandler<ToggledEventArgs>? Toggled;

    private static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ControlSwitch switchControl)
        {
            switchControl.UpdateThumbPosition(true);
            switchControl.Toggled?.Invoke(switchControl, new ToggledEventArgs((bool)newValue));
        }
    }

    private void UpdateThumbPosition(bool animated = true)
    {
        try
        {
            double gridWidth, gridHeight, thumbWidth, thumbHeight, margin, cornerRadius;
            
            switch (EffectiveSize)
            {
                case ComponentSize.Tablet:
                    gridWidth = 76;
                    gridHeight = 46;
                    thumbWidth = 40;
                    thumbHeight = 40;
                    margin = 6; // 3 * 2
                    cornerRadius = 23;
                    break;
                case ComponentSize.TV:
                    gridWidth = 102;
                    gridHeight = 62;
                    thumbWidth = 54;
                    thumbHeight = 54;
                    margin = 8; // 4 * 2
                    cornerRadius = 31;
                    break;
                default: // Phone
                    gridWidth = 51;
                    gridHeight = 31;
                    thumbWidth = 27;
                    thumbHeight = 27;
                    margin = 4; // 2 * 2
                    cornerRadius = 15;
                    break;
            }
            
            SwitchBorder.WidthRequest = gridWidth;
            SwitchBorder.HeightRequest = gridHeight;
            SwitchGrid.WidthRequest = gridWidth;
            SwitchGrid.HeightRequest = gridHeight;
            Thumb.WidthRequest = thumbWidth;
            Thumb.HeightRequest = thumbHeight;
            Thumb.Margin = new Thickness(margin / 2);
            BorderStrokeShape.CornerRadius = cornerRadius;
            
            double actualGridWidth = SwitchGrid.Width > 0 ? SwitchGrid.Width : gridWidth;
            double actualThumbWidth = Thumb.Width > 0 ? Thumb.Width : thumbWidth;
            double maxTranslation = actualGridWidth - actualThumbWidth - margin;
            double targetX = IsToggled ? maxTranslation : 0;

            if (animated)
            {
                _ = Thumb.TranslateToAsync(targetX, 0, 150, Easing.CubicInOut);
            }
            else
            {
                Thumb.TranslationX = targetX;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ControlSwitch.UpdateThumbPosition failed: {ex}");
        }
    }

    private void OnTapped(object sender, EventArgs e)
    {
        if (!Disabled)
        {
            IsToggled = !IsToggled;
        }
    }
}