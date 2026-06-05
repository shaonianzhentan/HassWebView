namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class Dialog : SizeableComponent
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Dialog), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(Dialog), string.Empty);

    public static readonly BindableProperty CancelTextProperty =
        BindableProperty.Create(nameof(CancelText), typeof(string), typeof(Dialog), "取消");

    public static readonly BindableProperty ConfirmTextProperty =
        BindableProperty.Create(nameof(ConfirmText), typeof(string), typeof(Dialog), "确定");

    public Dialog()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
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