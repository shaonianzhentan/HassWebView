namespace HassWebView.DemoComponent;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Console.WriteLine("Creating main window...");
        return new Window(new AppShell());
    }

    protected override void OnStart()
    {
        Console.WriteLine("App started");
        base.OnStart();
    }

    protected override void OnSleep()
    {
        Console.WriteLine("App sleeping");
        base.OnSleep();
    }

    protected override void OnResume()
    {
        Console.WriteLine("App resumed");
        base.OnResume();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Console.WriteLine($"[FATAL] Unhandled exception: {exception?.Message}");
        Console.WriteLine($"[FATAL] Stack trace: {exception?.StackTrace}");
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine($"[FATAL] Unobserved task exception: {e.Exception.Message}");
        Console.WriteLine($"[FATAL] Stack trace: {e.Exception.StackTrace}");
        e.SetObserved();
    }
}