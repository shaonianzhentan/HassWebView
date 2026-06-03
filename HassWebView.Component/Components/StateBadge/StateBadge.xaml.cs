namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class StateBadge : ContentView
{
    public StateBadge()
    {
        InitializeComponent();
        UpdateStateColor();
        UpdateSize();
        SizeManager.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateSize());
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null)
        {
            SizeManager.SizeChanged -= OnSizeChanged;
        }
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(StateBadge), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is StateBadge badge)
                    badge.UpdateStateColor();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    private void UpdateStateColor()
    {
        Color color = State?.ToLower() switch
        {
            "on" => Color.FromArgb("#34C759"),
            "open" => Color.FromArgb("#34C759"),
            "active" => Color.FromArgb("#34C759"),
            "locked" => Color.FromArgb("#34C759"),
            "off" => Color.FromArgb("#636366"),
            "closed" => Color.FromArgb("#636366"),
            "inactive" => Color.FromArgb("#636366"),
            "unlocked" => Color.FromArgb("#636366"),
            "unavailable" => Color.FromArgb("#FF3B30"),
            "unknown" => Color.FromArgb("#FF9500"),
            _ => Color.FromArgb("#636366")
        };
        BadgeBorder.BackgroundColor = color;
    }

    private void UpdateSize()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取字体大小
            if (resources.TryGetValue("ComponentBodySizeSmall", out var bodySmall) && bodySmall is double bodySmallVal)
                StateLabel.FontSize = bodySmallVal;
            
            // Padding
            if (resources.TryGetValue("ComponentPaddingSmall", out var padSmall) && padSmall is Thickness padSmallVal)
                BadgeBorder.Padding = padSmallVal;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StateBadge.UpdateSize failed: {ex}");
        }
    }
}