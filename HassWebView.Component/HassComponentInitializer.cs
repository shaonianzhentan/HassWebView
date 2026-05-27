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

        // 尝试加载资源字典，如果失败则忽略，避免阻塞应用启动
        try
        {
            var res = new Resources.ComponentResources();
            if (!application.Resources.MergedDictionaries.Any(x => x is Resources.ComponentResources))
            {
                application.Resources.MergedDictionaries.Add(res);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load ComponentResources: {ex.Message}");
        }
    }
}
