using System.ComponentModel;
using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class SwitchCard : ContentView
{
    public SwitchCard()
    {
        InitializeComponent();
    }

    // Event that consumers of the component can subscribe to
    public event EventHandler<ToggledEventArgs> Toggled;

    // BindableProperty for the Size of the component
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(SwitchCard), ComponentSize.Medium);

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    // BindableProperty for the Title
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SwitchCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // BindableProperty for the Description
    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(SwitchCard), string.Empty);

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // BindableProperty for the IsToggled state of the Switch
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(SwitchCard), false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnIsToggledChanged);

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }
    
    // When the IsToggled property changes, invoke the Toggled event
    private static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SwitchCard switchCard)
        {
            // Manually trigger the event handler for the switch itself to raise the public event.
            switchCard.OnSwitchToggled(switchCard, new ToggledEventArgs((bool)newValue));
        }
    }

    // This method is now the direct event handler for the Switch control in XAML
    private void OnSwitchToggled(object sender, ToggledEventArgs e)
    {
        // Raise the public Toggled event for consumers of the component
        Toggled?.Invoke(this, e);
    }
}
