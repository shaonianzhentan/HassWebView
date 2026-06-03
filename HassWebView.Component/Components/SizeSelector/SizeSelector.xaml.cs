namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SizeSelector : ContentView
{
    public SizeSelector()
    {
        InitializeComponent();
        
        // 延迟初始化以避免应用未完全启动时访问 Application.Current
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            UpdateButtonStates();
            SizeManager.SizeChanged += OnSizeChanged;
        });
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateButtonStates());
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null)
        {
            SizeManager.SizeChanged -= OnSizeChanged;
        }
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
            UpdateButtonStyle(MediumBtn, SizeManager.CurrentSize == ComponentSize.Phone);
            UpdateButtonStyle(LargeBtn, SizeManager.CurrentSize == ComponentSize.Tablet);
            UpdateButtonStyle(ExtraLargeBtn, SizeManager.CurrentSize == ComponentSize.TV);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeSelector.UpdateButtonStates failed: {ex}");
        }
    }

    private void UpdateButtonStyle(Button button, bool isSelected)
    {
        if (button == null) return;
        
        if (isSelected)
        {
            button.BackgroundColor = Color.FromArgb("#007AFF");
            button.TextColor = Colors.White;
        }
        else
        {
            button.BackgroundColor = Color.FromArgb("#F2F2F7");
            button.TextColor = Color.FromArgb("#636366");
        }
    }
}
