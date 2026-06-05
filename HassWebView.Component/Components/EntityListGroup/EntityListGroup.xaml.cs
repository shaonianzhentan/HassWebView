namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;

public partial class EntityListGroup : SizeableComponent
{
    public EntityListGroup()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EntityListGroup), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}