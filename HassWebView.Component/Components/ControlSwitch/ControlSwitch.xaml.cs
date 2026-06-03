namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ControlSwitch : ContentView
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(ControlSwitch), false,
            propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty DisabledProperty =
        BindableProperty.Create(nameof(Disabled), typeof(bool), typeof(ControlSwitch), false);

    public ControlSwitch()
    {
        InitializeComponent();
        UpdateSwitchVisual();
        UpdateSwitchSize();
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
            switchControl.UpdateSwitchVisual();
            switchControl.Toggled?.Invoke(switchControl, new ToggledEventArgs((bool)newValue));
        }
    }

    private void UpdateSwitchSize()
    {
        try
        {
            // WidthRequest 和 HeightRequest 根据尺寸设置
            switch (SizeManager.CurrentSize)
            {
                case Models.ComponentSize.Tablet:
                    SwitchGrid.WidthRequest = 76;
                    SwitchGrid.HeightRequest = 46;
                    Thumb.WidthRequest = 40;
                    Thumb.HeightRequest = 40;
                    break;
                case Models.ComponentSize.TV:
                    SwitchGrid.WidthRequest = 102;
                    SwitchGrid.HeightRequest = 62;
                    Thumb.WidthRequest = 54;
                    Thumb.HeightRequest = 54;
                    break;
                default: // Phone
                    SwitchGrid.WidthRequest = 51;
                    SwitchGrid.HeightRequest = 31;
                    Thumb.WidthRequest = 27;
                    Thumb.HeightRequest = 27;
                    break;
            }
            UpdateSwitchVisual();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ControlSwitch.UpdateSwitchSize failed: {ex}");
        }
    }

    private void UpdateSwitchVisual()
    {
        if (Disabled)
        {
            SwitchBorder.BackgroundColor = IsToggled 
                ? Color.FromArgb("#B3B3B3") 
                : Color.FromArgb("#EFEFF4");
            Thumb.Fill = Colors.White;
            Thumb.Opacity = 0.6;
        }
        else
        {
            SwitchBorder.BackgroundColor = IsToggled 
                ? Color.FromArgb("#007AFF") 
                : Color.FromArgb("#EFEFF4");
            Thumb.Fill = Colors.White;
            Thumb.Opacity = 1;
        }

        Thumb.TranslationX = IsToggled ? (SwitchGrid.WidthRequest - Thumb.WidthRequest - 4) : 0;
    }

    private void OnTapped(object sender, EventArgs e)
    {
        if (!Disabled)
        {
            IsToggled = !IsToggled;
        }
    }
}