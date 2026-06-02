using Com.Tencent.Smtt.Sdk;


namespace HassWebView.Core.Platforms.Android.TencentX5;

    public class TencentTbsListener : Java.Lang.Object, ITbsListener
    {
        public event EventHandler<int>? DownloadFinished;
        public event EventHandler<int>? DownloadProgress;
        public event EventHandler<int>? InstallFinished;

        public void OnDownloadFinish(int p0)
        {
            DownloadFinished?.Invoke(this, p0);
        }

        public void OnDownloadProgress(int p0)
        {
            DownloadProgress?.Invoke(this, p0);
        }

        public void OnInstallFinish(int p0)
        {
            InstallFinished?.Invoke(this, p0);
        }
    }