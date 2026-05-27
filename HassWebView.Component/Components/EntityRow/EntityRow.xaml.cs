namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class EntityRow : ContentView
{
    public EntityRow()
    {
        InitializeComponent();
        UpdateColors();
        UpdateSize();
        SizeManager.SizeChanged += (s, e) => UpdateSize();
    }

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(EntityRow), string.Empty);

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EntityRow), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(EntityRow), string.Empty,
            propertyChanged: (b, _, __) =>
            {
                if (b is EntityRow row)
                    row.UpdateColors();
            });

    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(EntityRow), false,
            propertyChanged: (b, _, __) =>
            {
                if (b is EntityRow row)
                    row.UpdateColors();
            });

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public Color IconBackgroundColor
    {
        get => (Color)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(EntityRow), Color.FromHex("#EFEFF4"));

    public Color StateColor
    {
        get => (Color)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(EntityRow), Color.FromHex("#636366"));

    private void UpdateColors()
    {
        if (IsActive || State?.ToLower() == "on")
        {
            IconBackgroundColor = Color.FromHex("#34C759");
            StateColor = Color.FromHex("#34C759");
        }
        else
        {
            IconBackgroundColor = Color.FromHex("#EFEFF4");
            StateColor = Color.FromHex("#636366");
        }
    }

    private void UpdateSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                IconBorder.Padding = new Thickness(10);
                IconLabel.FontSize = 20;
                TitleLabel.FontSize = 15;
                StateLabel.FontSize = 13;
                ArrowLabel.FontSize = 24;
                break;
            case ComponentSize.Tablet:
                IconBorder.Padding = new Thickness(16);
                IconLabel.FontSize = 32;
                TitleLabel.FontSize = 22;
                StateLabel.FontSize = 18;
                ArrowLabel.FontSize = 36;
                break;
            case ComponentSize.TV:
                IconBorder.Padding = new Thickness(24);
                IconLabel.FontSize = 44;
                TitleLabel.FontSize = 28;
                StateLabel.FontSize = 22;
                ArrowLabel.FontSize = 48;
                break;
        }
    }
}