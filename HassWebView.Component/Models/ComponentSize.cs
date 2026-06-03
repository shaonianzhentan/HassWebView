namespace HassWebView.Component.Models;

public enum ComponentSize
{
    Phone,
    Tablet,
    TV
}

public static class SizeManager
{
    public static ComponentSize CurrentSize { get; private set; } = ComponentSize.Phone;
    
    public static void SetSize(ComponentSize size)
    {
        CurrentSize = size;
    }
}
