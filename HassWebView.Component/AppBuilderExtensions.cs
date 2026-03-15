namespace HassWebView.Component
{
    public static class AppBuilderExtensions
    {
        public static MauiAppBuilder UseHassComponents(this MauiAppBuilder builder)
        {
            // Merges the component library's styles into the application's resources.
            builder.ConfigureMauiHandlers(handlers =>
            {
                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    mergedDictionaries.Add(new Styles.Styles());
                }
            });

            return builder;
        }
    }
}
