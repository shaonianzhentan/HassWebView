using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace HassWebView.Component;

[ContentProperty(nameof(Items))]
public partial class SettingsGroup : ContentView
{
    private readonly ObservableCollection<View> _items = new();

    public SettingsGroup()
    {
        InitializeComponent();
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>
    /// Child views placed directly in XAML. Supports SwitchCard, InfoRow, CheckBoxListItem, etc.
    /// </summary>
    public IList<View> Items => _items;

    #region Bindable Properties

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SettingsGroup), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is SettingsGroup g)
                    g.HeaderSection.IsVisible = !string.IsNullOrEmpty((string?)newValue);
            });

    /// <summary>
    /// Optional section title shown above the card.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty FooterProperty =
        BindableProperty.Create(nameof(Footer), typeof(string), typeof(SettingsGroup), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is SettingsGroup g)
                    g.FooterLabel.IsVisible = !string.IsNullOrEmpty((string?)newValue);
            });

    /// <summary>
    /// Optional footer description shown below the card.
    /// </summary>
    public string? Footer
    {
        get => (string?)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    #endregion

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildContainer();
    }

    private void RebuildContainer()
    {
        ItemsContainer.Children.Clear();

        var views = _items.ToList();
        for (int i = 0; i < views.Count; i++)
        {
            var view = views[i];
            bool isLast = i == views.Count - 1;

            // Wrap in padding container
            var wrapper = new ContentView
            {
                Padding = new Thickness(16, 0),
                Content = view
            };
            ItemsContainer.Children.Add(wrapper);

            // Auto-hide bottom line for last InfoRow / SwitchCard / CheckBoxListItem
            SetHideBottomLine(view, isLast);

            // Insert divider between items (not after last)
            if (!isLast)
            {
                ItemsContainer.Children.Add(CreateDivider(indent: 16));
            }
        }
    }

    private static void SetHideBottomLine(View view, bool hide)
    {
        switch (view)
        {
            case InfoRow infoRow:
                infoRow.HideBottomLine = hide;
                break;
            // SwitchCard and CheckBoxListItem don't have a built-in divider,
            // so dividers are managed externally by the container (inserted between items above).
        }
    }

    private static BoxView CreateDivider(double indent = 0)
    {
        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return new BoxView
        {
            HeightRequest = 0.5,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(indent, 0, 0, 0),
            Color = isDark
                ? Microsoft.Maui.Graphics.Color.FromArgb("#38383A")
                : Microsoft.Maui.Graphics.Color.FromArgb("#E5E5EA")
        };
    }
}
