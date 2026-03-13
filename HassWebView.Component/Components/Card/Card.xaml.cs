using HassWebView.Component.Models;
using Microsoft.Maui.Controls;

namespace HassWebView.Component
{
    [ContentProperty(nameof(Content))]
    public partial class Card : ContentView
    {
        public Card()
        {
            InitializeComponent();
        }

        // BindableProperty for the Size of the component
        public static readonly BindableProperty SizeProperty =
            BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(Card), ComponentSize.Medium);

        public ComponentSize Size
        {
            get => (ComponentSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        // BindableProperty for the Header content
        public static readonly BindableProperty HeaderProperty =
            BindableProperty.Create(nameof(Header), typeof(View), typeof(Card), null);

        public View Header
        {
            get => (View)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        // BindableProperty for the main Content
        public new static readonly BindableProperty ContentProperty =
            BindableProperty.Create(nameof(Content), typeof(View), typeof(Card), null);

        public new View Content
        {
            get => (View)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        // BindableProperty for the Footer content
        public static readonly BindableProperty FooterProperty =
            BindableProperty.Create(nameof(Footer), typeof(View), typeof(Card), null);

        public View Footer
        {
            get => (View)GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }
    }
}
