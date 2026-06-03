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
            SizeManager.SizeChanged += OnSizeManagerChanged;
        });
    }

    private void OnSizeManagerChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateSpinnerSize());
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null)
        {
            SizeManager.SizeChanged -= OnSizeManagerChanged;
        }
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
            
            // Scale factor 根据尺寸设置
            double scaleFactor = SizeManager.CurrentSize switch
            {
                Models.ComponentSize.Tablet => 1.4,
                Models.ComponentSize.TV => 1.8,
                _ => 1.0
            };
            
            SpinnerIndicator.Scale = scale * scaleFactor;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Spinner.UpdateSpinnerSize failed: {ex}");
        }
    }
}
