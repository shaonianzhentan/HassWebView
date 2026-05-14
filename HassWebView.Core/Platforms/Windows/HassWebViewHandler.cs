using HassWebView.Core.Bridges;
using HassWebView.Core.Events;
using HassWebView.Core.Models;
using HassWebView.Core.Services;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HassWebView.Core.Platforms.Windows;

using WebView = Microsoft.UI.Xaml.Controls.WebView2;

public class HassWebViewHandler : ViewHandler<HassWebView, WebView>
{
    private JsBridgeHandler _jsBridgeHandler;
    private string _pendingHtml;
    private string _pendingBaseUrl;

    // --- ADDED: Manual History Tracking for Windows ---
    private readonly List<HassWebHistoryItem> _history = new();
    private int _currentIndex = -1;
    private CoreWebView2NavigationKind _navigationKind;
    // --------------------------------------------------

    public static PropertyMapper Mapper = new PropertyMapper<HassWebView>()
    {
        [nameof(HassWebView.Source)] = (handler, view) =>
        {
            if (handler is not HassWebViewHandler h) return;
            h.LoadSource(view.Source);
        },
        [nameof(HassWebView.UserAgent)] = (handler, view) =>
        {
            if (handler.PlatformView is not WebView wv) return;
            if (string.IsNullOrEmpty(view.UserAgent)) return;
            if (wv.CoreWebView2 != null)
            {
                wv.CoreWebView2.Settings.UserAgent = view.UserAgent;
            }
        }
    };

    public static CommandMapper CommandMapper = new CommandMapper<HassWebView>
    {
        [nameof(HassWebView.GoBack)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoBack)
                wv.GoBack();
        },
        [nameof(HassWebView.GoForward)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv && wv.CanGoForward)
                wv.GoForward();
        },
        [nameof(HassWebView.Focus)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.Focus(FocusState.Programmatic);
            }
        },
        [nameof(HassWebView.Unfocus)] = (handler, view, args) =>
        {
            if (handler.PlatformView is WebView wv)
            {
                wv.Focus(FocusState.Unfocused);
            }
        },
        [nameof(HassWebView.EvaluateJavaScriptAsync)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.EvaluateJavaScriptAsyncRequest request) return;
            if (handler.PlatformView is not WebView wv) return;
            try
            {
                var result = await wv.ExecuteScriptAsync(request.Script);
                request.TaskCompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                request.TaskCompletionSource.SetException(ex);
            }
        },
        // --- MODIFIED: To use manual history ---
        [nameof(HassWebView.GetBackForwardListAsync)] = (handler, _, args) =>
        {
            if (args is not TaskCompletionSource<HassWebBackForwardList> tcs) return;
            if (handler is not HassWebViewHandler h)
            {
                tcs.SetResult(new HassWebBackForwardList { History = new List<HassWebHistoryItem>(), CurrentIndex = -1 });
                return;
            }

            var result = new HassWebBackForwardList
            {
                History = new List<HassWebHistoryItem>(h._history),
                CurrentIndex = h._currentIndex
            };

            tcs.SetResult(result);
        },
        // --------------------------------------
        [nameof(HassWebView.SimulateTouch)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchRequest request) return;
            if (handler.PlatformView is not WebView wv) return;
            var scriptTemplate = await ResourceHelper.GetResourceAsync("Scripts/SimulateTouch.js");
            var script = scriptTemplate
                .Replace("__VW__", wv.ActualWidth.ToString(CultureInfo.InvariantCulture))
                .Replace("__VH__", wv.ActualHeight.ToString(CultureInfo.InvariantCulture))
                .Replace("__X__", request.X.ToString(CultureInfo.InvariantCulture))
                .Replace("__Y__", request.Y.ToString(CultureInfo.InvariantCulture));
            await wv.ExecuteScriptAsync(script);
        },
        [nameof(HassWebView.SimulateTouchSlide)] = async (handler, _, args) =>
        {
            if (args is not HassWebView.SimulateTouchSlideRequest request) return;
            if (handler.PlatformView is not WebView wv) return;

            var scriptTemplate = await ResourceHelper.GetResourceAsync("Scripts/SimulateTouchSlide.js");
            var script = scriptTemplate
                .Replace("__VW__", wv.ActualWidth.ToString(CultureInfo.InvariantCulture))
                .Replace("__VH__", wv.ActualHeight.ToString(CultureInfo.InvariantCulture))
                .Replace("__X1__", request.X1.ToString(CultureInfo.InvariantCulture))
                .Replace("__Y1__", request.Y1.ToString(CultureInfo.InvariantCulture))
                .Replace("__X2__", request.X2.ToString(CultureInfo.InvariantCulture))
                .Replace("__Y2__", request.Y2.ToString(CultureInfo.InvariantCulture))
                .Replace("__DURATION__", request.Duration.ToString(CultureInfo.InvariantCulture));
            await wv.ExecuteScriptAsync(script);
        },
        [nameof(HassWebView.ExitFullscreen)] = async (handler, _, args) =>
        {
            if (handler.PlatformView is not WebView wv) return;
            await wv.ExecuteScriptAsync("if (document.fullscreenElement) { document.exitFullscreen(); }");
        }
    };

    public HassWebViewHandler() : base(Mapper, CommandMapper) { }

    protected override WebView CreatePlatformView()
    {
        return new WebView();
    }

    protected override async void ConnectHandler(WebView platformView)
    {
        base.ConnectHandler(platformView);
        _jsBridgeHandler = new JsBridgeHandler(VirtualView.JsBridges);
        platformView.WebMessageReceived += PlatformView_WebMessageReceived;
        platformView.CoreWebView2Initialized += PlatformView_CoreWebView2Initialized;
        await platformView.EnsureCoreWebView2Async();
    }

    private async void PlatformView_WebMessageReceived(WebView sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (_jsBridgeHandler != null)
        {
            await _jsBridgeHandler.HandleMessageAsync(args.WebMessageAsJson);
        }
    }

    private async void PlatformView_CoreWebView2Initialized(WebView sender, CoreWebView2InitializedEventArgs args)
    {
        var core = sender.CoreWebView2;
        if (!string.IsNullOrEmpty(VirtualView.UserAgent))
        {
            core.Settings.UserAgent = VirtualView.UserAgent;
        }
        core.Settings.IsWebMessageEnabled = true;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.NavigationStarting += Core_NavigationStarting;
        core.NavigationCompleted += Core_NavigationCompleted;
        core.NewWindowRequested += Core_NewWindowRequested;
        core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        core.WebResourceRequested += Core_WebResourceRequested;

        if (_jsBridgeHandler != null)
        {
            var proxyScript = _jsBridgeHandler.GenerateProxyScript();
            if (!string.IsNullOrEmpty(proxyScript))
            {
                await core.AddScriptToExecuteOnDocumentCreatedAsync(proxyScript);
            }
        }

        var injectedScript = await ResourceHelper.GetResourceAsync("Scripts/injected.js");
        if (!string.IsNullOrEmpty(injectedScript))
        {
            await core.AddScriptToExecuteOnDocumentCreatedAsync(injectedScript);
        }

        LoadSource(VirtualView.Source);
    }

    private void Core_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        args.Handled = true;
        PlatformView.CoreWebView2.Navigate(args.Uri);
    }

    private void Core_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
    {
        if (!string.IsNullOrEmpty(_pendingBaseUrl) && args.Request.Uri == _pendingBaseUrl)
        {
            var htmlBytes = Encoding.UTF8.GetBytes(_pendingHtml);
            var memoryStream = new MemoryStream(htmlBytes);
            var randomAccessStream = new InMemoryRandomAccessStream();
            using (var dataWriter = new DataWriter(randomAccessStream))
            {
                dataWriter.WriteBytes(htmlBytes);
                dataWriter.StoreAsync().GetAwaiter().GetResult();
                randomAccessStream.Seek(0);
            }

            var response = sender.Environment.CreateWebResourceResponse(
                randomAccessStream,
                200, "OK", "Content-Type: text/html; charset=utf-8");
            args.Response = response;
            _pendingHtml = null;
            _pendingBaseUrl = null;
            return;
        }

        var mauiArgs = new ResourceLoadingEventArgs(args.Request.Uri);
        if (VirtualView.SendResourceLoading(mauiArgs))
        {
            args.Response = sender.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "");
        }
    }

    private void Core_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        // --- ADDED: Store navigation kind to correctly update history in NavigationCompleted ---
        _navigationKind = args.NavigationKind;

        var mauiArgs = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, VirtualView.Source, args.Uri);
        VirtualView.SendNavigating(mauiArgs);
        args.Cancel = mauiArgs.Cancel;
    }

    private void Core_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        // --- ADDED: Manual history update logic ---
        if (args.IsSuccess)
        {
            switch (_navigationKind)
            {
                case CoreWebView2NavigationKind.BackOrForward:
                    _currentIndex = _history.FindIndex(item => item.Url == sender.Source);
                    break;

                case CoreWebView2NavigationKind.Reload:
                    // Do nothing with history
                    break;

                default: // New navigation
                    if (_currentIndex < _history.Count - 1)
                    {
                        _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));
                    }
                    
                    if (_currentIndex == -1 || _history[_currentIndex].Url != sender.Source)
                    {
                        _history.Add(new HassWebHistoryItem { Url = sender.Source, Title = sender.DocumentTitle });
                        _currentIndex = _history.Count - 1;
                    }
                    break;
            }
        }
        // ----------------------------------------

        var result = args.IsSuccess ? WebNavigationResult.Success : WebNavigationResult.Failure;
        var mauiArgs = new WebNavigatedEventArgs(WebNavigationEvent.NewPage, VirtualView.Source, sender.Source, result);
        VirtualView.SendNavigated(mauiArgs);
        VirtualView.CanGoBack = _currentIndex > 0;
        VirtualView.CanGoForward = _currentIndex < _history.Count - 1;
    }

    void LoadSource(WebViewSource source)
    {
        if (PlatformView?.CoreWebView2 == null)
            return;

        if (source is UrlWebViewSource urlSource)
        {
            _pendingHtml = null;
            _pendingBaseUrl = null;
            PlatformView.CoreWebView2.Navigate(urlSource.Url);
        }
        else if (source is HtmlWebViewSource htmlSource)
        {
            _pendingHtml = htmlSource.Html;
            _pendingBaseUrl = htmlSource.BaseUrl ?? "http://local.html";
            PlatformView.CoreWebView2.NavigateToString(htmlSource.Html);
        }
    }

    protected override void DisconnectHandler(WebView platformView)
    {
        platformView.WebMessageReceived -= PlatformView_WebMessageReceived;
        platformView.CoreWebView2Initialized -= PlatformView_CoreWebView2Initialized;
        if (platformView.CoreWebView2 != null)
        {
            platformView.CoreWebView2.NavigationStarting -= Core_NavigationStarting;
            platformView.CoreWebView2.NavigationCompleted -= Core_NavigationCompleted;
            platformView.CoreWebView2.NewWindowRequested -= Core_NewWindowRequested;
            platformView.CoreWebView2.WebResourceRequested -= Core_WebResourceRequested;
        }
        _jsBridgeHandler = null;

        // --- ADDED: Cleanup for manual history ---
        _history.Clear();
        _currentIndex = -1;
        // --------------------------------------

        base.DisconnectHandler(platformView);
    }
}
