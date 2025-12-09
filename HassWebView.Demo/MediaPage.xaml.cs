
using HassWebView.Core.Behaviors;
using HassWebView.Core.Events;
using HassWebView.Core.Services;

namespace HassWebView.Demo;

public partial class MediaPage : ContentPage, IKeyHandler
{
	public MediaPage(string url)
	{
		InitializeComponent();
        this.Behaviors.Add(new RemoteControlBehavior());
        Loaded += (_, _)=> {
            LoadUrl(url);
        };
    }

    void LoadUrl(string videoUrl){
        string htmlContent = $@"
                <html>
                <head>
                    <style>
                        body {{ margin: 0; padding: 0; background-color: black; }}
                        video {{
                            width: 100vw;
                            height: 100vh;
                            object-fit: contain; /* 保持比例，覆盖整个视口，可以改为 'cover' 如果想裁剪 */
                        }}
                    </style>
                </head>
                <body>
                    <video controls autoplay src='{videoUrl}'></video>
                </body>
                </html>";

            // 2. 创建 HtmlWebViewSource
            var htmlSource = new HtmlWebViewSource
            {
                Html = htmlContent,
                
                // BaseUrl 可以是 null 或 videoUrl 的域名，
                // 如果视频链接是绝对路径，这个属性影响不大，但最好设置以满足同源策略。
                BaseUrl = videoUrl.Contains("http") ? new Uri(videoUrl).GetLeftPart(UriPartial.Authority) : null
            };
            wv.Source = htmlSource;
    }

    public bool OnKeyDown(KeyService sender, RemoteKeyEventArgs args)
    {
        if (args.KeyName == "VolumeUp" || args.KeyName == "VolumeDown")
        {
            return false; // Let the system handle volume keys
        }
        return true; // We will handle all other keys
    }

    public void OnKeyUp(KeyService sender, RemoteKeyEventArgs args)
    {
        sender.StopRepeatingAction();
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
                    await Navigation.PopAsync();
                    break;

                case "Left":
                case "DpadLeft":
                    VideoService.VideoSeek(wv, -5); 
                    break;

                case "Right":
                case "DpadRight":
                    VideoService.VideoSeek(wv, 5); 
                    break;
            }
        });
    }
    
    public void OnDoubleClick(KeyService sender, RemoteKeyEventArgs args)
    {
        // No action 
    }


    public void OnLongClick(KeyService sender, RemoteKeyEventArgs e)
    {
        int repeatInterval = 100;
        switch (e.KeyName)
        {
            case "Left":
            case "DpadLeft":
                sender.StartRepeatingAction(() => VideoService.VideoSeek(wv, -15), repeatInterval);
                break;
            case "Right":
            case "DpadRight":
                sender.StartRepeatingAction(() => VideoService.VideoSeek(wv, 15), repeatInterval);
                break;
        }
    }
}
