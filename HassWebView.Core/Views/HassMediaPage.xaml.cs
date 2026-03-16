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
        Debug.WriteLine("[HassMediaPage] OnNavigatedTo: Subscribing to key events.");

        // Subscribe to events when navigation to this page is complete.
        _keyService.KeyDown += OnKeyDown;
        _keyService.SingleClick += OnSingleClick;
        _keyService.LongClick += OnLongClick;
        
        // Load the video content
        _ = LoadUrl(Url, BaseUrl);
    }
    
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        Debug.WriteLine("[HassMediaPage] OnNavigatedFrom: Unsubscribing from key events.");
        
        // Stop any repeating actions immediately when navigating away.
        _keyService.StopRepeatingAction();

        // Unsubscribe from events when leaving the page.
        _keyService.KeyDown -= OnKeyDown;
        _keyService.SingleClick -= OnSingleClick;
        _keyService.LongClick -= OnLongClick;
    }

    public async Task LoadUrl(string videoUrl, string baseUrl)
    {
        if (string.IsNullOrEmpty(videoUrl)) return;
        Debug.WriteLine($"Loading video URL: {videoUrl}");

        string htmlContent = await ResourceHelper.GetResourceAsync("MediaPlayer.html");

        var finalHtml = htmlContent.Replace("__HEIGHT__", wv.Height.ToString())
                                     .Replace("__VIDEO_URL__", videoUrl);

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
    }

    void VideoSeek(int second)
    {
        wv.EvaluateJavaScriptAsync($"videoSeek({second})");
    }

    void PlayPause()
    {
        wv.EvaluateJavaScriptAsync("playPause()");
    }

    public bool OnKeyDown(object sender, RemoteKeyEventArgs args)
    {
        if (args.NormalizedKeyName == "VolumeUp" || args.NormalizedKeyName == "VolumeDown")
        {
            return false;
        }
        return true;
    }

    public void OnSingleClick(object sender, RemoteKeyEventArgs e)
    {
        Debug.WriteLine($"[HassMediaPage] Single Click: {e.NormalizedKeyName}");
        
        switch (e.NormalizedKeyName)
        {
            case "Enter":
                PlayPause();
                break;

            case "Back":
                MainThread.BeginInvokeOnMainThread(() => Shell.Current.Navigation.PopModalAsync());
                break;

            case "Left":
                VideoSeek(-5);
                break;

            case "Right":
                VideoSeek(5);
                break;
        }
    }

    public void OnLongClick(object sender, RemoteKeyEventArgs e)
    {
        Debug.WriteLine($"[HassMediaPage] Long Click: {e.NormalizedKeyName}");
        int repeatInterval = 100;
        switch (e.NormalizedKeyName)
        {
            case "Left":
                _keyService.StartRepeatingAction(() => VideoSeek(-15), repeatInterval);
                break;
            case "Right":
                _keyService.StartRepeatingAction(() => VideoSeek(15), repeatInterval);
                break;
        }
    }
}
