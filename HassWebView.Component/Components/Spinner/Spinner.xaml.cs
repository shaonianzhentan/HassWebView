namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class Spinner : ContentView
{
    public static readonly BindableProperty IsRunningProperty =
        BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(Spinner), true);

    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(Spinner), Color.FromArgb("#007AFF"));

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Spinner), 48.0,
            propertyChanged: OnSizeChanged);

    public Spinner()
    {
        InitializeComponent();
        
        // 延迟初始化以避免应用未完全启动时访问 Application.Current
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            UpdateSpinnerSize();
            SizeManager.SizeChanged += (s, e) => UpdateSpinnerSize();
        });
    }

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Spinner spinner)
        {
            double size = (double)newValue;
            // 根据 Size 值设�?Scale（以 48 为基准）
            spinner.SpinnerIndicator.Scale = size / 48;
        }
    }

    private void UpdateSpinnerSize()
    {
        try
        {
            double baseSize = (double)GetValue(SizeProperty);
            double scale = baseSize / 48;
            
            switch (SizeManager.CurrentSize)
            {
                case ComponentSize.Phone:
                    SpinnerIndicator.Scale = scale * 1.0;
                    break;
                case ComponentSize.Tablet:
                    SpinnerIndicator.Scale = scale * 1.4;
                    break;
                case ComponentSize.TV:
                    SpinnerIndicator.Scale = scale * 1.8;
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Spinner.UpdateSpinnerSize failed: {ex}");
        }
    }
}
