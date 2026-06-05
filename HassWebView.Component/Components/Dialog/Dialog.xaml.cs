namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

[ContentProperty(nameof(Content))]
public partial class Dialog : AdaptiveComponent
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Dialog), string.Empty, propertyChanged: OnTitlePropertyChanged);

    public static readonly BindableProperty CancelTextProperty =
        BindableProperty.Create(nameof(CancelText), typeof(string), typeof(Dialog), "取消");

    public static readonly BindableProperty ConfirmTextProperty =
        BindableProperty.Create(nameof(ConfirmText), typeof(string), typeof(Dialog), "确定");

    public static readonly BindableProperty HasTitleProperty =
        BindableProperty.Create(nameof(HasTitle), typeof(bool), typeof(Dialog), false);

    public static new readonly BindableProperty ContentProperty =
        BindableProperty.Create(nameof(Content), typeof(View), typeof(Dialog), null);

    public Dialog()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public string ConfirmText
    {
        get => (string)GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    public bool HasTitle
    {
        get => (bool)GetValue(HasTitleProperty);
        private set => SetValue(HasTitleProperty, value);
    }

    public new View Content
    {
        get => (View)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private static void OnTitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Dialog dialog)
        {
            dialog.HasTitle = !string.IsNullOrEmpty((string)newValue);
        }
    }

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public void Show()
    {
        this.IsVisible = true;
    }

    public void Hide()
    {
        this.IsVisible = false;
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        Hide();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Hide();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        Hide();
        Confirmed?.Invoke(this, EventArgs.Empty);
    }
}