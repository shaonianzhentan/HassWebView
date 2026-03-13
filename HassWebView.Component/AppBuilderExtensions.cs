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
                var an = typeof(AppBuilderExtensions).Assembly.GetName().Name;
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/{an};component/Styles.xaml", UriKind.Relative)
                });
            });

            return builder;
        }
    }
}
