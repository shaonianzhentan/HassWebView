using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using HassWebView.Core.Events;

namespace HassWebView.Core;

public class HassWebView : WebView
{
    public HassWebView()
    {
        JsBridges = new Dictionary<string, object>();
    }

    public static readonly BindableProperty JsBridgesProperty =
        BindableProperty.Create(nameof(JsBridges), typeof(IDictionary<string, object>), typeof(HassWebView), null);

    public IDictionary<string, object> JsBridges
    {
        get => (IDictionary<string, object>)GetValue(JsBridgesProperty);
        set => SetValue(JsBridgesProperty, value);
    }

    public static readonly BindableProperty CanGoBackProperty =
        BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(HassWebView), false);

    public bool CanGoBack
    {
        get => (bool)GetValue(CanGoBackProperty);
        internal set => SetValue(CanGoBackProperty, value);
    }

    public static readonly BindableProperty CanGoForwardProperty =
        BindableProperty.Create(nameof(CanGoForward), typeof(bool), typeof(HassWebView), false);

    public bool CanGoForward
    {
        get => (bool)GetValue(CanGoForwardProperty);
        internal set => SetValue(CanGoForwardProperty, value);
    }

    private static readonly BindablePropertyKey IsVideoFullscreenPropertyKey =
        BindableProperty.CreateReadOnly(nameof(IsVideoFullscreen), typeof(bool), typeof(HassWebView), false);

    public static readonly BindableProperty IsVideoFullscreenProperty = IsVideoFullscreenPropertyKey.BindableProperty;

    public bool IsVideoFullscreen => (bool)GetValue(IsVideoFullscreenProperty);

    public event EventHandler<bool> VideoPlayingFullscreen;

    internal void SetIsVideoFullscreen(bool isVideoFullscreen)
    {
        SetValue(IsVideoFullscreenPropertyKey, isVideoFullscreen);
        VideoPlayingFullscreen?.Invoke(this, isVideoFullscreen);
    }


    public event EventHandler<WebNavigatingEventArgs> Navigating;
    public event EventHandler<WebNavigatedEventArgs> Navigated;
    public event EventHandler<ResourceLoadingEventArgs> ResourceLoading;

    internal void SendNavigating(WebNavigatingEventArgs args)
    {
        Navigating?.Invoke(this, args);
    }

    internal void SendNavigated(WebNavigatedEventArgs args)
    {
        Navigated?.Invoke(this, args);
    }

    internal bool SendResourceLoading(ResourceLoadingEventArgs args)
    {
        ResourceLoading?.Invoke(this, args);
        return args.Cancel;
    }

    public new Task<string> EvaluateJavaScriptAsync(string script)
    {
        var tcs = new TaskCompletionSource<string>();

        if (Handler == null)
        {
            tcs.SetException(new InvalidOperationException("Handler is not initialized."));
            return tcs.Task;
        }

        Handler.Invoke(nameof(EvaluateJavaScriptAsync), new EvaluateJavaScriptAsyncRequest(tcs, script));
        return tcs.Task;
    }
    
    public void SimulateTouch(int x, int y)
    {
        if (Handler == null)
        {
            return;
        }

        Handler.Invoke(nameof(SimulateTouch), new SimulateTouchRequest(x, y));
    }

    public void SimulateTouchSlide(int x1, int y1, int x2, int y2, int duration)
    {
        if (Handler == null)
        {
            return;
        }

        Handler.Invoke(nameof(SimulateTouchSlide), new SimulateTouchSlideRequest(x1, y1, x2, y2, duration));
    }
    public void ExitFullscreen()
    {
        if (Handler == null) return;
        Handler.Invoke(nameof(ExitFullscreen));
    }

    internal class EvaluateJavaScriptAsyncRequest
    {
        public EvaluateJavaScriptAsyncRequest(TaskCompletionSource<string> tcs, string script)
        {
            TaskCompletionSource = tcs;
            Script = script;
        }

        public TaskCompletionSource<string> TaskCompletionSource { get; }
        public string Script { get; }
    }

    internal class SimulateTouchRequest
    {
        public SimulateTouchRequest(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    internal class SimulateTouchSlideRequest
    {
        public SimulateTouchSlideRequest(int x1, int y1, int x2, int y2, int duration)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Duration = duration;
        }

        public int X1 { get; }
        public int Y1 { get; }
        public int X2 { get; }
        public int Y2 { get; }
        public int Duration { get; }
    }
}
