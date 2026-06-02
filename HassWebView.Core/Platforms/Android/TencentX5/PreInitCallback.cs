using Com.Tencent.Smtt.Sdk;


namespace HassWebView.Core.Platforms.Android.TencentX5;

public class PreInitCallback : Java.Lang.Object, QbSdk.IPreInitCallback
{
    public event EventHandler? CoreInitFinished;
    public event EventHandler<bool>? ViewInitFinished;

    public void OnCoreInitFinished()
    {
        CoreInitFinished?.Invoke(this, EventArgs.Empty);
    }

    public void OnViewInitFinished(bool p0)
    {
        ViewInitFinished?.Invoke(this, p0);
    }
}