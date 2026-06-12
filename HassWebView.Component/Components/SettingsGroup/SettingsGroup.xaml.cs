namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SettingsGroup : VerticalStackLayout
{
    private readonly Border _mainBorder;
    private readonly VerticalStackLayout _contentLayout;
    private readonly Label _titleLabel;

    private ComponentSize _effectiveSize = SizeManager.CurrentSize;

    #region Bindable Properties

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize?), typeof(SettingsGroup), null,
            propertyChanged: OnSizePropertyChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SettingsGroup), string.Empty);

    public ComponentSize? Size
    {
        get => (ComponentSize?)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ComponentSize EffectiveSize
    {
        get => _effectiveSize;
        private set
        {
            if (_effectiveSize != value)
            {
                _effectiveSize = value;
                OnPropertyChanged(nameof(EffectiveSize));
            }
        }
    }

    #endregion

    public SettingsGroup()
    {
        // ---- 标题 ----
        _titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
        };
        _titleLabel.SetDynamicResource(Label.TextColorProperty, "TertiaryTextColor");
        _titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

        // ---- 内容布局（放标题 + 子元素） ----
        _contentLayout = new VerticalStackLayout();
        _contentLayout.Children.Add(_titleLabel);

        // ---- 边框卡片 ----
        _mainBorder = new Border
        {
            Content = _contentLayout,
            StrokeThickness = 1,
        };
        _mainBorder.SetDynamicResource(Border.BackgroundColorProperty, "CardBackgroundColor");
        _mainBorder.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };
        var strokeBrush = new SolidColorBrush();
        strokeBrush.SetDynamicResource(SolidColorBrush.ColorProperty, "BorderColor");
        _mainBorder.Stroke = strokeBrush;

        // ---- 将 Border 作为 SettingsGroup 的第一个子元素 ----
        this.Children.Add(_mainBorder);

        // ---- 初始化 ----
        InitializeComponent();
        UpdateEffectiveSize();
        UpdateTitleStyle();
        SizeManager.SizeChanged += OnGlobalSizeChanged;
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (child is View view && view != _mainBorder)
        {
            // 用户添加的子元素移到 Border 内的布局中
            this.Children.Remove(view);
            _contentLayout.Children.Add(view);
        }
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        if (child is View view)
        {
            _contentLayout.Children.Remove(view);
        }
    }

    #region Adaptive Sizing

    private static void OnSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SettingsGroup group)
        {
            group.UpdateEffectiveSize();
            group.UpdateTitleStyle();
        }
    }

    private void OnGlobalSizeChanged(object? sender, EventArgs e)
    {
        UpdateEffectiveSize();
        UpdateTitleStyle();
    }

    private void UpdateEffectiveSize()
    {
        EffectiveSize = Size ?? SizeManager.CurrentSize;
    }

    private void UpdateTitleStyle()
    {
        double fontSize;
        Thickness padding;

        switch (EffectiveSize)
        {
            case ComponentSize.Tablet:
                fontSize = 18;
                padding = new Thickness(14, 10);
                break;
            case ComponentSize.TV:
                fontSize = 24;
                padding = new Thickness(18, 14);
                break;
            default:
                fontSize = 12;
                padding = new Thickness(10, 6);
                break;
        }

        _titleLabel.FontSize = fontSize;
        _titleLabel.Padding = padding;
    }

    #endregion
}
