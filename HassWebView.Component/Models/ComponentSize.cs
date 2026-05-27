namespace HassWebView.Component.Models;

public enum ComponentSize
{
    Phone,
    Tablet,
    TV
}

public static class SizeManager
{
    public static ComponentSize CurrentSize { get; set; } = ComponentSize.Phone;
    
    public static event EventHandler? SizeChanged;
    
    public static void SetSize(ComponentSize size)
    {
        CurrentSize = size;
        SizeChanged?.Invoke(null, EventArgs.Empty);
    }
}