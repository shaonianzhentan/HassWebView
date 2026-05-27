namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class Alert : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Alert), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(Alert), string.Empty);

    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(AlertType), typeof(Alert), AlertType.Warning,
            propertyChanged: OnTypeChanged);

    public Alert()
    {
        InitializeComponent();
        UpdateAlertStyle();
        UpdateAlertSize();
        SizeManager.SizeChanged += (s, e) => UpdateAlertSize();
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

    public AlertType Type
    {
        get => (AlertType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    private void UpdateAlertSize()
    {
        switch (SizeManager.CurrentSize)
        {
            case ComponentSize.Phone:
                IconLabel.FontSize = 20;
                TitleLabel.FontSize = 14;
                MessageLabel.FontSize = 13;
                CloseBtn.FontSize = 14;
                AlertBorder.Padding = new Thickness(16);
                break;
            case ComponentSize.Tablet:
                IconLabel.FontSize = 28;
                TitleLabel.FontSize = 18;
                MessageLabel.FontSize = 16;
                CloseBtn.FontSize = 18;
                AlertBorder.Padding = new Thickness(20);
                break;
            case ComponentSize.TV:
                IconLabel.FontSize = 36;
                TitleLabel.FontSize = 24;
                MessageLabel.FontSize = 20;
                CloseBtn.FontSize = 24;
                AlertBorder.Padding = new Thickness(24);
                break;
        }
    }

    private void UpdateAlertStyle()
    {
        switch (Type)
        {
            case AlertType.Warning:
                AlertBorder.BackgroundColor = Color.FromHex("#FFF8E6");
                IconLabel.Text = "⚠️";
                TitleLabel.TextColor = Color.FromHex("#1D1D1F");
                MessageLabel.TextColor = Color.FromHex("#3C3C3C");
                break;
            case AlertType.Error:
                AlertBorder.BackgroundColor = Color.FromHex("#FFEBEB");
                IconLabel.Text = "❌";
                TitleLabel.TextColor = Color.FromHex("#FF3B30");
                MessageLabel.TextColor = Color.FromHex("#CC1A00");
                break;
            case AlertType.Success:
                AlertBorder.BackgroundColor = Color.FromHex("#E8F8E8");
                IconLabel.Text = "✅";
                TitleLabel.TextColor = Color.FromHex("#30D158");
                MessageLabel.TextColor = Color.FromHex("#00C853");
                break;
            case AlertType.Info:
                AlertBorder.BackgroundColor = Color.FromHex("#E8F4FD");
                IconLabel.Text = "ℹ️";
                TitleLabel.TextColor = Color.FromHex("#007AFF");
                MessageLabel.TextColor = Color.FromHex("#0066CC");
                break;
        }
    }

    private static void OnTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Alert alert)
        {
            alert.UpdateAlertStyle();
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
    }
}

public enum AlertType
{
    Warning,
    Error,
    Success,
    Info
}