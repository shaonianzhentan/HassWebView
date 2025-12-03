using Android.Widget;
using Com.Tencent.Smtt.Export.External.Interfaces;
using Com.Tencent.Smtt.Sdk;
using Microsoft.Maui.Platform;
using AndroidViews = Android.Views;

namespace HassWebView.Core.Platforms.Android;

class WebChromeClientHandler : WebChromeClient
{
    private readonly HassWebView _webView;
    public AndroidViews.View _customView;
    private IX5WebChromeClientCustomViewCallback _customViewCallback;

    public WebChromeClientHandler(HassWebView webView)
    {
        _webView = webView;
    }

    public override void OnPermissionRequest(IPermissionRequest request)
    {
        request.Grant(request.GetResources());
    }

    public override void OnShowCustomView(AndroidViews.View view, IX5WebChromeClientCustomViewCallback callback)
    {
        if (_customView != null)
        {
            callback.OnCustomViewHidden();
            return;
        }

        _customView = view;
        _customViewCallback = callback;
        var activity = Platform.CurrentActivity;
        var decorView = (FrameLayout)activity.Window.DecorView;

        decorView.AddView(_customView, new FrameLayout.LayoutParams(
            AndroidViews.ViewGroup.LayoutParams.MatchParent,
            AndroidViews.ViewGroup.LayoutParams.MatchParent));

        _webView.SetIsVideoFullscreen(true);
    }

    public override void OnHideCustomView()
    {
        if (_customView == null)
        {
            return;
        }

        var activity = Platform.CurrentActivity;
        var decorView = (FrameLayout)activity.Window.DecorView;
        decorView.RemoveView(_customView);
        _customView = null;
        _customViewCallback?.OnCustomViewHidden();
        _customViewCallback = null;
        _webView.SetIsVideoFullscreen(false);
    }
}
