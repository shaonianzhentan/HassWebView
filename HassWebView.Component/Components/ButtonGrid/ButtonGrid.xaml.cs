using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace HassWebView.Component;

[ContentProperty(nameof(Items))]
public partial class ButtonGrid : ContentView
{
    private readonly ObservableCollection<View> _items = new();

    public ButtonGrid()
    {
        InitializeComponent();
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>
    /// Child ButtonCard views placed directly in XAML.
    /// </summary>
    public IList<View> Items => _items;

    #region Bindable Properties

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ButtonGrid), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is ButtonGrid g)
                    g.HeaderSection.IsVisible = !string.IsNullOrEmpty((string?)newValue);
            });

    /// <summary>
    /// Optional section title shown above the grid.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns), typeof(int), typeof(ButtonGrid), 2,
            propertyChanged: (b, _, _) => ((ButtonGrid)b).RebuildGrid());

    /// <summary>
    /// Number of columns in the grid. Default is 2.
    /// </summary>
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    #endregion

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildGrid();
    }

    private void RebuildGrid()
    {
        ButtonsGrid.Children.Clear();
        ButtonsGrid.ColumnDefinitions.Clear();
        ButtonsGrid.RowDefinitions.Clear();

        int cols = Math.Max(1, Columns);
        var views = _items.ToList();

        if (views.Count == 0)
            return;

        // Build column definitions (equal width)
        for (int c = 0; c < cols; c++)
            ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // Calculate row count
        int rows = (int)Math.Ceiling((double)views.Count / cols);
        for (int r = 0; r < rows; r++)
            ButtonsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Place each item
        for (int i = 0; i < views.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;
            var view = views[i];
            Grid.SetRow(view, row);
            Grid.SetColumn(view, col);
            ButtonsGrid.Children.Add(view);
        }
    }
}
