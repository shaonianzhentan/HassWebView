namespace HassWebView.Demo;

/// <summary>
/// This page acts as a host for the HassAuthenticatedView.
/// It receives a URL and an optional Source via query parameters and passes the URL to the view.
/// </summary>
[QueryProperty(nameof(Url), "url")]
[QueryProperty(nameof(Source), "source")] // Add this line
public partial class WebPage : ContentPage
{
    /// <summary>
    /// Receives the 'url' query parameter from the navigation URI.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Receives the 'source' query parameter (e.g., "login").
    /// </summary>
    public string Source { get; set; } // Add this property

    public WebPage()
    {
        InitializeComponent();

        // When the view signals a logout, navigate back to the app's main page.
        authView.LoggedOut += (s, e) =>
        {
            Dispatcher.Dispatch(() => Shell.Current.GoToAsync("//MainPage"));
        };
    }

    /// <summary>
    /// Called when the page has been navigated to.
    /// </summary>
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // You could potentially use the 'Source' property here for different logic
        // For example: if (Source == "login") { ... }

        // Pass the URL from the navigation parameter to our view.
        // The view will then handle loading, authentication, and security.
        if (!string.IsNullOrWhiteSpace(Url))
        {
            authView.SourceUrl = Url;
        }
    }
}
