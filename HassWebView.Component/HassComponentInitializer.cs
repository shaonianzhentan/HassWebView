using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace HassWebView.Component;

public class HassComponentInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider serviceProvider)
    {
        var application = serviceProvider.GetService<Application>() ?? Application.Current;
        if (application?.Resources?.MergedDictionaries == null)
        {
            return;
        }

        if (!application.Resources.MergedDictionaries.Any(x => x is Resources.ComponentResources))
        {
            application.Resources.MergedDictionaries.Add(new Resources.ComponentResources());
        }
    }
}
