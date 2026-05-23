using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace HassWebView.Component;

[ContentProperty(nameof(Items))]
public partial class EntityListGroup : ContentView
{
    private readonly ObservableCollection<View> _items = new();

    public EntityListGroup()
    {
        InitializeComponent();
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>
    /// Fired when the header action label is tapped.
    /// </summary>
    public event EventHandler? ActionTapped;

    /// <summary>
    /// Child EntityRow views placed directly in XAML.
    /// </summary>
    public IList<View> Items => _items;

    #region Bindable Properties

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EntityListGroup), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is EntityListGroup g)
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

    public static readonly BindableProperty ActionProperty =
        BindableProperty.Create(nameof(Action), typeof(string), typeof(EntityListGroup), null);

    /// <summary>
    /// Optional action text shown on the right of the section header.
    /// </summary>
    public string? Action
    {
        get => (string?)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
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

            // Each EntityRow gets horizontal padding
            var wrapper = new ContentView
            {
                Padding = new Thickness(16, 0),
                Content = view
            };
            ItemsContainer.Children.Add(wrapper);

            // Insert divider between rows (not after the last)
            if (!isLast)
            {
                ItemsContainer.Children.Add(CreateDivider(indent: 68)); // align with text, skipping icon column
            }
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

    private void OnHeaderActionTapped(object? sender, EventArgs e)
    {
        ActionTapped?.Invoke(this, EventArgs.Empty);
    }
}
