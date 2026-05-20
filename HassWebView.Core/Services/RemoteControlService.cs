using HassWebView.Core.Controls;
using System.Diagnostics;
using System.Web;

namespace HassWebView.Core.Services;

/// <summary>
/// Defines the interface for the remote control service, which manages the active WebView control for remote commands.
/// </summary>
public interface IRemoteControlService
{
    void SetActiveControl(WebViewWithCursor control);
    void ClearActiveControl(WebViewWithCursor control);
}

/// <summary>
/// Implementation of the remote control service. This is a singleton service.
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

                case "slideup":
                    _activeControl._cursorControl.SlideUp();
                    break;

                case "slidedown":
                    _activeControl._cursorControl.SlideDown();
                    break;

                case "slideleft":
                    _activeControl._cursorControl.SlideLeft();
                    break;

                case "slideright":
                    _activeControl._cursorControl.SlideRight();
                    break;

                case "text":
                    var append = query["append"] == "1";
                    var text = query["text"];
                    if (_activeControl.WebViewControl != null)
                    {
                        await _activeControl.WebViewControl.Web.InsertTextAsync(text, append);
                    }
                    break;
                case "key":
                    var key = query["key"];
                    if (_activeControl.WebViewControl != null)
                    {
                        await _activeControl.WebViewControl.Web.SimulateKeyPressAsync(key);
                    }
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
