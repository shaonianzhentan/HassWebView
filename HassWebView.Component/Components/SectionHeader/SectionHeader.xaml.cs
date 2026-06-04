namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SectionHeader : ContentView
{
    public SectionHeader()
    {
        InitializeComponent();
        UpdateSize();
    }

    public event EventHandler? ActionTapped;

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SectionHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ActionProperty =
        BindableProperty.Create(nameof(Action), typeof(string), typeof(SectionHeader), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is SectionHeader header)
                    header.ActionLabel.IsVisible = !string.IsNullOrEmpty((string?)newValue);
            });

    public string? Action
    {
        get => (string?)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    private void UpdateSize()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取字体大小
            if (resources.TryGetValue("ComponentTitleSizeSmall", out var titleSmall) && titleSmall is double titleSmallVal)
            {
                TitleLabel.FontSize = titleSmallVal;
                ActionLabel.FontSize = titleSmallVal;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SectionHeader.UpdateSize failed: {ex}");
        }
    }

    private void OnActionTapped(object sender, TappedEventArgs e)
    {
        ActionTapped?.Invoke(this, EventArgs.Empty);
    }
}