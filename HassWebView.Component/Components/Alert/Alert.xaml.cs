namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class Alert : ContentView
{
    private bool _isInitialized = false;

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
        
        // 延迟初始化以避免应用未完全启动时访问 Application.Current
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            try
            {
                UpdateAlertStyle();
                UpdateAlertSize();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Alert initialization failed: {ex}");
            }
        });
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
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取字体大小
            if (resources.TryGetValue("ComponentIconSizePhoneSmall", out var iconSmall) && iconSmall is double iconSmallVal)
                IconLabel.FontSize = iconSmallVal;
            
            if (resources.TryGetValue("ComponentTitleSizeMedium", out var titleMed) && titleMed is double titleMedVal)
                TitleLabel.FontSize = titleMedVal;
            
            if (resources.TryGetValue("ComponentBodySizeMedium", out var bodyMed) && bodyMed is double bodyMedVal)
                MessageLabel.FontSize = bodyMedVal;
            
            if (resources.TryGetValue("ComponentTitleSizeLarge", out var titleLg) && titleLg is double titleLgVal)
                CloseBtn.FontSize = titleLgVal;
            
            // Padding
            if (resources.TryGetValue("ComponentPaddingMedium", out var padMed) && padMed is Thickness padMedVal)
                AlertBorder.Padding = padMedVal;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Alert.UpdateAlertSize failed: {ex}");
        }
    }

    private void UpdateAlertStyle()
    {
        try
        {
            switch (Type)
            {
                case AlertType.Warning:
                    AlertBorder.BackgroundColor = Color.FromArgb("#FFF8E6");
                    IconLabel.Text = "⚠️";
                    TitleLabel.TextColor = Color.FromArgb("#1D1D1F");
                    MessageLabel.TextColor = Color.FromArgb("#3C3C3C");
                    break;
                case AlertType.Error:
                    AlertBorder.BackgroundColor = Color.FromArgb("#FFEBEB");
                    IconLabel.Text = "❌";
                    TitleLabel.TextColor = Color.FromArgb("#FF3B30");
                    MessageLabel.TextColor = Color.FromArgb("#CC1A00");
                    break;
                case AlertType.Success:
                    AlertBorder.BackgroundColor = Color.FromArgb("#E8F8E8");
                    IconLabel.Text = "✅";
                    TitleLabel.TextColor = Color.FromArgb("#30D158");
                    MessageLabel.TextColor = Color.FromArgb("#00C853");
                    break;
                case AlertType.Info:
                    AlertBorder.BackgroundColor = Color.FromArgb("#E8F4FD");
                    IconLabel.Text = "ℹ️";
                    TitleLabel.TextColor = Color.FromArgb("#007AFF");
                    MessageLabel.TextColor = Color.FromArgb("#0066CC");
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Alert.UpdateAlertStyle failed: {ex}");
        }
    }

    private static void OnTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Alert alert)
        {
            // 只在组件初始化完成后才更新样�?
            if (alert._isInitialized)
            {
                alert.UpdateAlertStyle();
            }
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
