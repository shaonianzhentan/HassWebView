namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class DetailCard : ContentView
{
    public DetailCard()
    {
        InitializeComponent();
        
        // 延迟初始化以避免应用未完全启动时访问 Application.Current
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            UpdateStateColor();
            UpdateSize();
            SizeManager.SizeChanged += (s, e) => UpdateSize();
        });
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DetailCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(DetailCard), string.Empty);

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(DetailCard), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is DetailCard card)
                    card.UpdateStateColor();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public Color StateColor
    {
        get => (Color)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(DetailCard), Color.FromArgb("#34C759"));

    private void UpdateStateColor()
    {
        try
        {
            Color color = State?.ToLower() switch
            {
                "on" => Color.FromArgb("#34C759"),
                "open" => Color.FromArgb("#34C759"),
                "off" => Color.FromArgb("#8E8E93"),
                "closed" => Color.FromArgb("#8E8E93"),
                "unavailable" => Color.FromArgb("#FF3B30"),
                _ => Color.FromArgb("#34C759")
            };
            StateColor = color;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DetailCard.UpdateStateColor failed: {ex}");
        }
    }

    private void UpdateSize()
    {
        try
        {
            switch (SizeManager.CurrentSize)
            {
                case ComponentSize.Phone:
                    CardFrame.Padding = new Thickness(20);
                    TitleLabel.FontSize = 16;
                    SubtitleLabel.FontSize = 14;
                    StateFrame.Padding = new Thickness(12, 6);
                    StateLabel.FontSize = 14;
                    break;
                case ComponentSize.Tablet:
                    CardFrame.Padding = new Thickness(28);
                    TitleLabel.FontSize = 24;
                    SubtitleLabel.FontSize = 18;
                    StateFrame.Padding = new Thickness(20, 10);
                    StateLabel.FontSize = 20;
                    break;
                case ComponentSize.TV:
                    CardFrame.Padding = new Thickness(36);
                    TitleLabel.FontSize = 32;
                    SubtitleLabel.FontSize = 24;
                    StateFrame.Padding = new Thickness(28, 14);
                    StateLabel.FontSize = 28;
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DetailCard.UpdateSize failed: {ex}");
        }
    }
}
