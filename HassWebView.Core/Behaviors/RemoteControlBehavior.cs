using HassWebView.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HassWebView.Core.Behaviors;

public class RemoteControlBehavior : Behavior<Page>
{
    private KeyService _keyService;
    private IKeyHandler _keyHandler;
    private Page _associatedPage;

    protected override void OnAttachedTo(Page page)
    {
        base.OnAttachedTo(page);
        _associatedPage = page;

        if (page is not IKeyHandler keyHandler)
        {
            return;
        }

        _keyHandler = keyHandler;
        page.HandlerChanged += OnHandlerChanged;

        // Attempt registration immediately in case the handler is already available.
        TryRegister();
    }

    protected override void OnDetachingFrom(Page page)
    {
        page.HandlerChanged -= OnHandlerChanged;
        TryUnregister();

        _associatedPage = null;
        _keyHandler = null;
        _keyService = null;

        base.OnDetachingFrom(page);
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
        TryRegister();
    }

    private void TryRegister()
    {
        // Do not proceed if the handler is not yet available, or if registration has already occurred.
        if (_associatedPage?.Handler == null || _keyService != null)
        {
            return;
        }

#if ANDROID || WINDOWS
        var keyService = _associatedPage.Handler.MauiContext?.Services.GetService<KeyService>();
#endif

        if (keyService != null)
        {
            _keyService = keyService;
            _keyService.Register(_keyHandler);
        }
    }

    private void TryUnregister()
    {
        if (_keyService != null)
        {
            _keyService.Unregister();
        }
    }
}
