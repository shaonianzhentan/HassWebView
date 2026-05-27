using HassWebView.DemoComponent.Views;

namespace HassWebView.DemoComponent;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        AddDemoPageButtons();
    }

    private void AddDemoPageButtons()
    {
        var pages = new (string Title, Type PageType)[]
        {
            ("Chip 示例页", typeof(ChipExamplePage)),
            ("ButtonCard 示例页", typeof(ButtonCardExamplePage)),
            ("Alert 示例页", typeof(AlertExamplePage)),
            ("ControlSwitch 示例页", typeof(ControlSwitchExamplePage)),
            ("DetailCard 示例页", typeof(DetailCardExamplePage)),
            ("EntityRow 示例页", typeof(EntityRowExamplePage)),
            ("Gauge 示例页", typeof(GaugeExamplePage)),
            ("Input 示例页", typeof(InputExamplePage)),
            ("SliderCard 示例页", typeof(SliderCardExamplePage)),
            ("Spinner 示例页", typeof(SpinnerExamplePage)),
            ("StateBadge 示例页", typeof(StateBadgeExamplePage)),
            ("ThemeSelector 示例页", typeof(ThemeSelectorExamplePage)),
        };

        foreach (var page in pages)
        {
            var button = new Button
            {
                Text = page.Title,
                CornerRadius = 12,
                BackgroundColor = Colors.LightGray,
                TextColor = Colors.Black,
                HeightRequest = 48
            };
            button.Clicked += async (_, _) =>
            {
                if (Activator.CreateInstance(page.PageType) is Page pageInstance)
                {
                    await Navigation.PushAsync(pageInstance);
                }
            };
            ButtonsLayout.Add(button);
        }
    }
}
