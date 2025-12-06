using HassWebView.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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
            Debug.WriteLine("[RemoteControlBehavior] Attached page does not implement IKeyHandler.");
            return;
        }
        _keyHandler = keyHandler;

        page.HandlerChanged += OnHandlerChanged;

        // In case the handler is already available when this behavior is attached.
        TryInitializeAndRegister();
    }

    protected override void OnDetachingFrom(Page page)
    {
        page.HandlerChanged -= OnHandlerChanged;

        if (_keyService != null)
        {
            // Unregister the handler to prevent memory leaks and dangling references.
            _keyService.Unregister();
        }

        _associatedPage = null;
        _keyHandler = null;
        _keyService = null;

        base.OnDetachingFrom(page);
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
        TryInitializeAndRegister();
    }

    private void TryInitializeAndRegister()
    {
        // We only proceed if the handler is set but our keyService hasn't been acquired yet.
        if (_associatedPage?.Handler == null || _keyService != null)
        {
            return;
        }

        var keyService = _associatedPage.Handler.MauiContext?.Services.GetService<KeyService>();


        if (keyService != null)
        {    
            Debug.WriteLine("[RemoteControlBehavior] KeyService acquired. Registering handler.");
            _keyService = keyService;
            _keyService.Register(_keyHandler);
        }
        else
        {
            Debug.WriteLine("[RemoteControlBehavior] Could not acquire KeyService from DI container.");
        }
    }
}
