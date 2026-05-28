using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace HassWebView.Component;

public class HassComponentInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider serviceProvider)
    {
        try
        {
            var application = serviceProvider.GetService<Application>() ?? Application.Current;
            if (application?.Resources?.MergedDictionaries == null)
            {
                System.Diagnostics.Debug.WriteLine("Application or Resources is null");
                return;
            }

            // 延迟加载资源字典，避免在应用初始化期间访问资源
            application.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
            {
                try
                {
                    var colorsRes = new ResourceDictionary();
                    colorsRes.Source = new Uri("Resources/Styles/Colors.xaml", UriKind.Relative);
                    application.Resources.MergedDictionaries.Add(colorsRes);
                    System.Diagnostics.Debug.WriteLine("Colors.xaml loaded successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load Colors.xaml: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HassComponentInitializer.Initialize failed: {ex.Message}");
        }
    }
}
