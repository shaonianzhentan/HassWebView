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
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取尺寸值
            if (resources.TryGetValue("ComponentPaddingLarge", out var padLg) && padLg is Thickness padLgVal)
                CardFrame.Padding = padLgVal;
            
            if (resources.TryGetValue("ComponentTitleSizeLarge", out var titleLg) && titleLg is double titleLgVal)
                TitleLabel.FontSize = titleLgVal;
            
            if (resources.TryGetValue("ComponentBodySizeLarge", out var bodyLg) && bodyLg is double bodyLgVal)
                SubtitleLabel.FontSize = bodyLgVal;
            
            if (resources.TryGetValue("ComponentBodySizeMedium", out var bodyMed) && bodyMed is double bodyMedVal)
                StateLabel.FontSize = bodyMedVal;
            
            // StateFrame padding 使用特定值
            StateFrame.Padding = SizeManager.CurrentSize switch
            {
                Models.ComponentSize.Tablet => new Thickness(20, 10),
                Models.ComponentSize.TV => new Thickness(28, 14),
                _ => new Thickness(12, 6)
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DetailCard.UpdateSize failed: {ex}");
        }
    }
}
