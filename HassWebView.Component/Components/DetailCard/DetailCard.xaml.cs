using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace HassWebView.Component;

[ContentProperty(nameof(InfoRows))]
public partial class DetailCard : ContentView
{
    private readonly ObservableCollection<View> _infoRows = new();

    public DetailCard()
    {
        InitializeComponent();
        _infoRows.CollectionChanged += OnInfoRowsChanged;
    }

    /// <summary>
    /// InfoRow views placed directly in XAML (ContentProperty).
    /// The last row automatically has HideBottomLine = true.
    /// </summary>
    public IList<View> InfoRows => _infoRows;

    #region Bindable Properties

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DetailCard), string.Empty);

    /// <summary>
    /// Main title shown in the card header.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(DetailCard), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is DetailCard card)
                    card.SubtitleLabel.IsVisible = !string.IsNullOrEmpty((string?)newValue);
            });

    /// <summary>
    /// Optional subtitle shown below the title (e.g. device model or room name).
    /// </summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(string), typeof(DetailCard), "off");

    /// <summary>
    /// Entity state string that drives the StateBadge color in the header.
    /// </summary>
    public string State
    {
        get => (string)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly BindableProperty ActionContentProperty =
        BindableProperty.Create(nameof(ActionContent), typeof(View), typeof(DetailCard), null,
            propertyChanged: (b, _, newValue) =>
            {
                if (b is DetailCard card)
                {
                    card.ActionPresenter.Content = newValue as View;
                    card.ActionPresenter.IsVisible = newValue is not null;
                }
            });

    /// <summary>
    /// Optional view placed at the bottom of the card (e.g. a SliderCard).
    /// Set via DetailCard.ActionContent in XAML.
    /// </summary>
    public View? ActionContent
    {
        get => (View?)GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    #endregion

    private void OnInfoRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildInfoContainer();
    }

    private void RebuildInfoContainer()
    {
        InfoContainer.Children.Clear();

        var views = _infoRows.ToList();
        for (int i = 0; i < views.Count; i++)
        {
            var view = views[i];
            bool isLast = i == views.Count - 1;

            // Auto-set HideBottomLine on InfoRow
            if (view is InfoRow infoRow)
                infoRow.HideBottomLine = isLast;

            InfoContainer.Children.Add(view);
        }
    }
}
