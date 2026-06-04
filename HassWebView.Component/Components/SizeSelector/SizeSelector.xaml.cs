namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SizeSelector : ContentView
{
    public static readonly BindableProperty IsPhoneSelectedProperty = BindableProperty.Create(
        nameof(IsPhoneSelected), typeof(bool), typeof(SizeSelector), false);

    public static readonly BindableProperty IsTabletSelectedProperty = BindableProperty.Create(
        nameof(IsTabletSelected), typeof(bool), typeof(SizeSelector), false);

    public static readonly BindableProperty IsTVSelectedProperty = BindableProperty.Create(
        nameof(IsTVSelected), typeof(bool), typeof(SizeSelector), false);

    public bool IsPhoneSelected
    {
        get => (bool)GetValue(IsPhoneSelectedProperty);
        set => SetValue(IsPhoneSelectedProperty, value);
    }

    public bool IsTabletSelected
    {
        get => (bool)GetValue(IsTabletSelectedProperty);
        set => SetValue(IsTabletSelectedProperty, value);
    }

    public bool IsTVSelected
    {
        get => (bool)GetValue(IsTVSelectedProperty);
        set => SetValue(IsTVSelectedProperty, value);
    }

    public SizeSelector()
    {
        InitializeComponent();
        
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            UpdateButtonStates();
        });
    }

    private void OnSizeClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.CommandParameter is string sizeStr)
            {
                if (Enum.TryParse<ComponentSize>(sizeStr, out var size))
                {
                    SizeManager.SetSize(size);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.OnSizeClicked failed: {ex}");
        }
    }

    private void UpdateButtonStates()
    {
        try
        {
            IsPhoneSelected = SizeManager.CurrentSize == ComponentSize.Phone;
            IsTabletSelected = SizeManager.CurrentSize == ComponentSize.Tablet;
            IsTVSelected = SizeManager.CurrentSize == ComponentSize.TV;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.UpdateButtonStates failed: {ex}");
        }
    }
}