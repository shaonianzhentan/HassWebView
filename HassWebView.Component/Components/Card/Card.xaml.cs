using HassWebView.Component.Models;
using Microsoft.Maui.Controls;

namespace HassWebView.Component
{
    public partial class Card : Border // Changed base class from Frame to Border
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
            BindableProperty.Create(nameof(Header), typeof(object), typeof(Card), null);

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        // BindableProperty for the main Content
        public new static readonly BindableProperty ContentProperty =
            BindableProperty.Create(nameof(Content), typeof(object), typeof(Card), null);

        public new object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        // BindableProperty for the Footer content
        public static readonly BindableProperty FooterProperty =
            BindableProperty.Create(nameof(Footer), typeof(object), typeof(Card), null);

        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }
    }
}
