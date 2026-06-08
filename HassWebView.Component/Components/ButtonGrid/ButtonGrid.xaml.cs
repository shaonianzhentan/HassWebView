namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class ButtonGrid : Grid
{
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize?), typeof(ButtonGrid), null,
            propertyChanged: OnSizePropertyChanged);

    public static readonly BindableProperty ColumnCountProperty =
        BindableProperty.Create(nameof(ColumnCount), typeof(int), typeof(ButtonGrid), 2,
            propertyChanged: OnColumnCountChanged);

    private ComponentSize _effectiveSize = SizeManager.CurrentSize;

    public ComponentSize? Size
    {
        get => (ComponentSize?)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    public ComponentSize EffectiveSize
    {
        get => _effectiveSize;
        private set
        {
            if (_effectiveSize != value)
            {
                _effectiveSize = value;
                OnPropertyChanged(nameof(EffectiveSize));
            }
        }
    }

    public ButtonGrid()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeManager.SizeChanged += OnGlobalSizeChanged;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        UpdateEffectiveSize();
        UpdateSpacing();
        UpdateGridLayout();
        Loaded -= OnLoaded;
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        UpdateGridLayout();
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        UpdateGridLayout();
    }

    private static void OnSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ButtonGrid grid)
        {
            grid.UpdateEffectiveSize();
            grid.UpdateSpacing();
        }
    }

    private static void OnColumnCountChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ButtonGrid grid)
        {
            grid.UpdateGridLayout();
        }
    }

    private void OnGlobalSizeChanged(object? sender, EventArgs e)
    {
        UpdateEffectiveSize();
        UpdateSpacing();
    }

    private void UpdateEffectiveSize()
    {
        EffectiveSize = Size ?? SizeManager.CurrentSize;
    }

    private void UpdateSpacing()
    {
        switch (EffectiveSize)
        {
            case ComponentSize.Tablet:
                RowSpacing = 20;
                ColumnSpacing = 20;
                break;
            case ComponentSize.TV:
                RowSpacing = 32;
                ColumnSpacing = 32;
                break;
            default: // Phone
                RowSpacing = 12;
                ColumnSpacing = 12;
                break;
        }
    }

    private void UpdateGridLayout()
    {
        int columns = ColumnCount;
        int childrenCount = Children.Count;
        
        if (columns <= 0 || childrenCount == 0)
            return;

        int rows = (int)Math.Ceiling((double)childrenCount / columns);
        
        ColumnDefinitions.Clear();
        for (int i = 0; i < columns; i++)
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        
        RowDefinitions.Clear();
        for (int i = 0; i < rows; i++)
        {
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }
        
        for (int i = 0; i < childrenCount; i++)
        {
            int row = i / columns;
            int col = i % columns;
            SetRow(Children[i], row);
            SetColumn(Children[i], col);
        }
    }
}