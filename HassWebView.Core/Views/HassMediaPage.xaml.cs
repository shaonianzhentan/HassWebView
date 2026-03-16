using HassWebView.Core.Events;
using HassWebView.Core.Interfaces;
using HassWebView.Core.Services;
using System.Diagnostics;

namespace HassWebView.Core.Views;

public partial class HassMediaPage : ContentPage, IKeyHandler
{
    public string BaseUrl { get; set; }
    public string Url { get; set; }

    private readonly KeyService _keyService;

    public HassMediaPage()
    {
        InitializeComponent();
        // The KeyService is still needed for starting/stopping repeating actions.
        _keyService = IPlatformApplication.Current.Services.GetRequiredService<KeyService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadUrl(Url, BaseUrl);
    }

    protected override void OnDisappearing()
    {
        // Ensure any running repeating actions are stopped when the page is no longer visible.
        _keyService.StopRepeatingAction();
        base.OnDisappearing();
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

    private void VideoSeek(int second)
    {
        wv.EvaluateJavaScriptAsync($"videoSeek({second})");
    }

    private void PlayPause()
    {
        wv.EvaluateJavaScriptAsync("playPause()");
    }

    #region IKeyHandler Implementation

    public string[] GetUnhandledKeys() => new string[] { "VolumeUp", "VolumeDown" };

    public void OnSingleClick(RemoteKeyEventArgs args)
    {
        Debug.WriteLine($"[HassMediaPage] Single Click: {args.KeyName}");
        
        switch (args.KeyName)
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

    public void OnLongClick(RemoteKeyEventArgs args)
    {
        Debug.WriteLine($"[HassMediaPage] Long Click: {args.KeyName}");
        const int repeatInterval = 100;
        switch (args.KeyName)
        {
            case "Left":
                _keyService.StartRepeatingAction(() => VideoSeek(-15), repeatInterval);
                break;
            case "Right":
                _keyService.StartRepeatingAction(() => VideoSeek(15), repeatInterval);
                break;
        }
    }

    #endregion
}
