using Microsoft.Maui.Controls;

namespace HassWebView.Component
{
    public static class AppBuilderExtensions
    {
        public static MauiAppBuilder UseHassComponents(this MauiAppBuilder builder)
        {
            // Merges the component library's styles into the application's resources.
            builder.ConfigureMauiHandlers(handlers =>
            {
                Application.Current.Resources.MergedDictionaries.Add(new Styles());
            });

            return builder;
        }
    }
}
