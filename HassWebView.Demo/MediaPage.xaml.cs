
using HassWebView.Core.Events;
using HassWebView.Core.Services;

namespace HassWebView.Demo;

public partial class MediaPage : ContentPage, IQueryAttributable, IKeyHandler
{
	public MediaPage()
	{
		InitializeComponent();
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.ContainsKey("url"))
		{
			string? url = query["url"]?.ToString();
			if (string.IsNullOrEmpty(url)) return;

            VideoService.HtmlWebView(wv, System.Web.HttpUtility.UrlDecode(url));
		}
	}

    public void OnDoubleClick(KeyService sender, RemoteKeyEventArgs args)
    {

    }

    public bool OnKeyDown(KeyService sender, RemoteKeyEventArgs args)
    {
        return true;
    }

    public void OnKeyUp(KeyService sender, RemoteKeyEventArgs args)
    {

        sender.StopRepeatingAction();
    }

    public void OnLongClick(KeyService sender, RemoteKeyEventArgs args)
    {

    }

    public void OnSingleClick(KeyService sender, RemoteKeyEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            switch (e.KeyName)
            {
                case "Enter":
                case "DpadCenter":
                    await VideoService.TogglePlayPause(wv);
                    break;

                case "Escape":
                case "Back":
                    await Shell.Current.GoToAsync("..");
                    break;

                case "Up":
                case "DpadUp":

                    break;

                case "Down":
                case "DpadDown":

                    break;

                case "Left":
                case "DpadLeft":
                    VideoService.VideoSeek(wv, -5); break;

                case "Right":
                case "DpadRight":
                    VideoService.VideoSeek(wv, 5); break;
            }
        });

    }
}