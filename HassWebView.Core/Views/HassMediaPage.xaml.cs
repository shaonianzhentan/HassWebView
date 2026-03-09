using HassWebView.Core.Events;
using HassWebView.Core.Services;
using System.Diagnostics;

namespace HassWebView.Core.Views;

public partial class HassMediaPage : ContentPage
{
    public string BaseUrl { get; set; }
    public string Url { get; set; }

    private readonly KeyService _keyService;

    public HassMediaPage()
    {
        InitializeComponent();
        _keyService = IPlatformApplication.Current.Services.GetRequiredService<KeyService>();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        LoadUrl(Url, BaseUrl);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _keyService.KeyDown += OnKeyDown;
        _keyService.SingleClick += OnSingleClick;
        _keyService.LongClick += OnLongClick;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _keyService.KeyDown -= OnKeyDown;
        _keyService.SingleClick -= OnSingleClick;
        _keyService.LongClick -= OnLongClick;
    }

    public void LoadUrl(string videoUrl, string baseUrl)
    {
        if (string.IsNullOrEmpty(videoUrl)) return;
        Debug.WriteLine($"Loading video URL: {videoUrl}");

        string htmlContent = $@"\n                <html>\n                <head>\n                    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0'>\n                    <style>\n                        html,body {{ margin: 0; padding: 0; height: {wv.Height}px; background-color: black; }}\n                        video {{ width: 100%; height: 100%; object-fit: contain;  }}\n                    </style>\n                </head>\n                <body>\n                    <video controls autoplay src='{videoUrl}'></video>\n                </body>\n                </html>";

        var uri = new Uri(videoUrl);
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = $"{uri.Scheme}://{uri.Host}/";
        }
        var htmlSource = new HtmlWebViewSource
        {
            BaseUrl = baseUrl,
            Html = htmlContent
        };
        Debug.WriteLine("Setting WebView source with HTML content.");
        wv.Source = htmlSource;
    }

    void VideoSeek(int second)
    {
        wv.EvaluateJavaScriptAsync($@"(function() {{\n                var video = document.querySelector('video');\n                if (video) video.currentTime += {second};\n            }})()");
    }

    void PlayPause()
    {
        wv.EvaluateJavaScriptAsync(@"(function() {\n                var video = document.querySelector('video');\n                if (video) {\n                    if (video.paused) {\n                        video.play();\n                    } else {\n                        video.pause();\n                    }\n                }\n            })()");
    }

    public bool OnKeyDown(object sender, RemoteKeyEventArgs args)
    {
        if (args.KeyName == "VolumeUp" || args.KeyName == "VolumeDown")
        {
            return false;
        }
        return true;
    }

    public void OnSingleClick(object sender, RemoteKeyEventArgs e)
    {
        MainThread.InvokeOnMainThreadAsync(async () =>
        {
            switch (e.KeyName)
            {
                case "Enter":
                case "DpadCenter":
                    PlayPause();
                    break;

                case "Escape":
                case "Back":
                    await Shell.Current.Navigation.PopModalAsync();
                    break;

                case "Left":
                case "DpadLeft":
                    VideoSeek(-5);
                    break;

                case "Right":
                case "DpadRight":
                    VideoSeek(5);
                    break;
            }
        });
    }

    public void OnLongClick(object sender, RemoteKeyEventArgs e)
    {
        int repeatInterval = 100;
        switch (e.KeyName)
        {
            case "Left":
            case "DpadLeft":
                _keyService.StartRepeatingAction(() => VideoSeek(-15), repeatInterval);
                break;
            case "Right":
            case "DpadRight":
                _keyService.StartRepeatingAction(() => VideoSeek(15), repeatInterval);
                break;
        }
    }
}
