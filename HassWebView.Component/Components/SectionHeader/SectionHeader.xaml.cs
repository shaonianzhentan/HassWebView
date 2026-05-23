using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class SectionHeader : ContentView
{
    public SectionHeader()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Fired when the optional action label is tapped.
    /// </summary>
    public event EventHandler? ActionTapped;

    #region Bindable Properties

    /// <summary>
    /// The section title text (rendered uppercase with letter spacing).
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SectionHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Optional action text shown on the right. Setting this makes the action label visible.
    /// </summary>
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

    /// <summary>
    /// Controls the size variant (currently affects padding via parent context).
    /// </summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(SectionHeader), ComponentSize.Medium);

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    #endregion

    private void OnActionTapped(object sender, TappedEventArgs e)
    {
        ActionTapped?.Invoke(this, EventArgs.Empty);
    }
}
