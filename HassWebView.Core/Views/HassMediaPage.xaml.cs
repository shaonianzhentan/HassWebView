using HassWebView.Core.Events;
using HassWebView.Core.Services;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
        // Fire and forget is okay here
        _ = LoadUrl(Url, BaseUrl);
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

    public async Task LoadUrl(string videoUrl, string baseUrl)
    {
        if (string.IsNullOrEmpty(videoUrl)) return;
        Debug.WriteLine($"Loading video URL: {videoUrl}");

        string htmlContent = await ResourceHelper.GetResourceAsync("MediaPlayer.html");

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var finalHtml = htmlContent.Replace("{{HEIGHT}}", wv.Height.ToString())
                                         .Replace("{{VIDEO_URL}}", videoUrl);

            var uri = new Uri(videoUrl);
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = $"{uri.Scheme}://{uri.Host}/";
            }

            var htmlSource = new HtmlWebViewSource
            {
                BaseUrl = baseUrl,
                Html = finalHtml
            };

            Debug.WriteLine("Setting WebView source with HTML content from MediaPlayer.html.");
            wv.Source = htmlSource;
        });
    }

    void VideoSeek(int second)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            wv.EvaluateJavaScriptAsync($"videoSeek({second})");
        });
    }

    void PlayPause()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            wv.EvaluateJavaScriptAsync("playPause()");
        });
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
