using HassWebView.Core.Controls;
using System.Diagnostics;
using System.Web;

namespace HassWebView.Core.Services;

/// <summary>
/// 定义远程控制服务的接口，用于注册和清除活动的 WebView 控件。
/// </summary>
public interface IRemoteControlService
{
    /// <summary>
    /// 设置当前活动的、可被远程控制的 WebViewWithCursor 实例。
    /// </summary>
    /// <param name="control">当前在界面上可见的控件实例。</param>
    void SetActiveControl(WebViewWithCursor control);

    /// <summary>
    /// 当控件从界面移除时，清除对它的引用。
    /// </summary>
    /// <param name="control">不再可见的控件实例。</param>
    void ClearActiveControl(WebViewWithCursor control);
}

/// <summary>
/// 远程控制服务的实现类。这是一个单例服务。
/// </summary>
public class RemoteControlService : IRemoteControlService
{
    private WebViewWithCursor _activeControl;

    public RemoteControlService(HttpServer httpServer)
    {
        if (httpServer is null) return;

        Debug.WriteLine("[RemoteControlService] Registering remote control HTTP routes.");

        httpServer.Get("/webview/remote", async (req, res) =>
        {
            var htmlContent = await ResourceHelper.GetResourceAsync("remote.html");
            await res.Html(htmlContent);
        });

        httpServer.Get("/webview/config", async (req, res) =>
        {
            if (_activeControl != null)
                await res.Json(new { width = _activeControl.WebViewControl.Width });
            else
                await res.Json(new { width = 0 });
        });

        httpServer.Post("/webview/remote", async (req, res) =>
        {
            if (_activeControl?._cursorControl is null)
            {
                await res.Text("No active control available for remote commands.");
                return;
            }

            var query = HttpUtility.ParseQueryString(await req.BodyAsync());
            var type = query["type"];

            switch (type)
            {
                case "move":
                    _activeControl._cursorControl.MoveBy(Convert.ToDouble(query["x"]), Convert.ToDouble(query["y"]));
                    break;
                case "click":
                    _activeControl._cursorControl.Click();
                    break;
                case "text":
                    var append = query["append"] == "1";
                    var text = query["text"];
                    await ResourceHelper.ExecuteScriptAsync(_activeControl.WebViewControl, "Scripts/TextInput.js", $"HassTextInput.insert(\'{text.Replace("\'", "\\\'")}\', {append.ToString().ToLower()});");
                    break;
                default:
                    Debug.WriteLine($"[RemoteControlService] Unknown remote command type: {type}");
                    break;
            }
            await res.Text("");
        });
    }

    public void SetActiveControl(WebViewWithCursor control)
    {
        Debug.WriteLine($"[RemoteControlService] Active control set: {control.GetType().Name} ({control.GetHashCode()})");
        _activeControl = control;
    }

    public void ClearActiveControl(WebViewWithCursor control)
    {
        if (_activeControl == control)
        {
            Debug.WriteLine($"[RemoteControlService] Active control cleared: {control.GetType().Name} ({control.GetHashCode()})");
            _activeControl = null;
        }
    }
}
