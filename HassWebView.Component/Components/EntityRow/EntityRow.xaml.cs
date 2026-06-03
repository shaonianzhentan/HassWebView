namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class EntityRow : ContentView
{
    private bool _isInitialized = false;

    public EntityRow()
    {
        InitializeComponent();
        
        // 延迟初始化以避免应用未完全启动时访问 Application.Current
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            try
            {
                UpdateColors();
                UpdateSize();
                SizeManager.SizeChanged += OnSizeChanged;
                ThemeManager.ThemeChanged += OnThemeChanged;
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EntityRow initialization failed: {ex}");
            }
        });
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateSize());
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateColors());
    }

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);
        if (args.OldHandler != null)
        {
            SizeManager.SizeChanged -= OnSizeChanged;
            ThemeManager.ThemeChanged -= OnThemeChanged;
        }
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
                    row.SafeUpdateColors();
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
                    row.SafeUpdateColors();
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
        BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(EntityRow), Color.FromArgb("#EFEFF4"));

    public Color StateColor
    {
        get => (Color)GetValue(StateColorProperty);
        set => SetValue(StateColorProperty, value);
    }

    public static readonly BindableProperty StateColorProperty =
        BindableProperty.Create(nameof(StateColor), typeof(Color), typeof(EntityRow), Color.FromArgb("#636366"));

    private void SafeUpdateColors()
    {
        if (_isInitialized)
        {
            UpdateColors();
        }
    }

    private void UpdateColors()
    {
        try
        {
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            
            if (IsActive || State?.ToLower() == "on")
            {
                IconBackgroundColor = Color.FromArgb("#34C759");
                StateColor = Color.FromArgb("#34C759");
            }
            else
            {
                IconBackgroundColor = isDark ? Color.FromArgb("#3A3A3C") : Color.FromArgb("#EFEFF4");
                StateColor = isDark ? Color.FromArgb("#8E8E93") : Color.FromArgb("#636366");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EntityRow.UpdateColors failed: {ex}");
        }
    }

    private void UpdateSize()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            // 从资源字典读取尺寸值
            if (resources.TryGetValue("ComponentIconSizePhoneSmall", out var iconSmall) && iconSmall is double iconSmallVal)
                IconLabel.FontSize = iconSmallVal;
            
            if (resources.TryGetValue("ComponentTitleSizePhoneLarge", out var titleLg) && titleLg is double titleLgVal)
                TitleLabel.FontSize = titleLgVal;
            
            if (resources.TryGetValue("ComponentBodySizePhoneMedium", out var bodyMed) && bodyMed is double bodyMedVal)
                StateLabel.FontSize = bodyMedVal;
            
            if (resources.TryGetValue("ComponentIconSizePhoneLarge", out var iconLg) && iconLg is double iconLgVal)
                ArrowLabel.FontSize = iconLgVal;
            
            // IconBorder padding
            if (resources.TryGetValue("ComponentPaddingSmall", out var padSmall) && padSmall is Thickness padSmallVal)
                IconBorder.Padding = padSmallVal;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EntityRow.UpdateSize failed: {ex}");
        }
    }
}
