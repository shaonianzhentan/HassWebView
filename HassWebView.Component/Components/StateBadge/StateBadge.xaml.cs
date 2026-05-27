namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class StateBadge : ContentView
{
    public StateBadge()
    {
        InitializeComponent();
        UpdateStateColor();
        UpdateSize();
        SizeManager.SizeChanged += (s, e) => UpdateSize();
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
            "on" => Color.FromHex("#34C759"),
            "open" => Color.FromHex("#34C759"),
            "active" => Color.FromHex("#34C759"),
            "locked" => Color.FromHex("#34C759"),
            "off" => Color.FromHex("#8E8E93"),
            "closed" => Color.FromHex("#8E8E93"),
            "inactive" => Color.FromHex("#8E8E93"),
            "unlocked" => Color.FromHex("#8E8E93"),
            "unavailable" => Color.FromHex("#FF3B30"),
            "unknown" => Color.FromHex("#FF9500"),
            _ => Color.FromHex("#8E8E93")
        };
        BadgeBorder.BackgroundColor = color;
    }

    private void UpdateSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                BadgeBorder.Padding = new Thickness(10, 6);
                StateLabel.FontSize = 11;
                break;
            case ComponentSize.Tablet:
                BadgeBorder.Padding = new Thickness(16, 10);
                StateLabel.FontSize = 16;
                break;
            case ComponentSize.TV:
                BadgeBorder.Padding = new Thickness(24, 14);
                StateLabel.FontSize = 24;
                break;
        }
    }
}