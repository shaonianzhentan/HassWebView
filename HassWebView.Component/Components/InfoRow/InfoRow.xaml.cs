using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class InfoRow : ContentView
{
    public InfoRow()
    {
        InitializeComponent();
    }

    #region Bindable Properties

    /// <summary>
    /// The label shown on the left side.
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(InfoRow), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The value shown on the right side.
    /// </summary>
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(InfoRow), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// When true, hides the bottom divider line (useful for the last row in a group).
    /// </summary>
    public static readonly BindableProperty HideBottomLineProperty =
        BindableProperty.Create(nameof(HideBottomLine), typeof(bool), typeof(InfoRow), false,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is InfoRow row)
                    row.DividerLine.IsVisible = !(bool)newValue;
            });

    public bool HideBottomLine
    {
        get => (bool)GetValue(HideBottomLineProperty);
        set => SetValue(HideBottomLineProperty, value);
    }

    /// <summary>
    /// Controls the font sizes of the row.
    /// </summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(InfoRow), ComponentSize.Medium);

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    #endregion
}
