using Android.Widget;
using Com.Tencent.Smtt.Export.External.Interfaces;
using Com.Tencent.Smtt.Sdk;
using AndroidViews = Android.Views;

namespace Maui.HassWebView.Core.Platforms.Android;

class WebChromeClientHandler : WebChromeClient
{

    public override void OnPermissionRequest(IPermissionRequest request)
    {
        request.Grant(request.GetResources());
    }
    
    public override void OnShowCustomView(AndroidViews.View view, IX5WebChromeClientCustomViewCallback callback)
    {
        // 如果是 VideoView
        if (view is FrameLayout frame)
        {
            if (frame.FocusedChild != null)
            {

                // 这里无法直接拿 URL，通常用 WebViewClient 先拦截 URL
                // 所以 OnShowCustomView 可以隐藏 X5 播放控件
                callback.OnCustomViewHidden();
            }
        }
    }

    public override void OnHideCustomView()
    {
        base.OnHideCustomView();
    }

}
