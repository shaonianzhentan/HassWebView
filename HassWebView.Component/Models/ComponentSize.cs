namespace HassWebView.Component.Models;

using Microsoft.Maui.Controls;

public enum ComponentSize
{
    Phone,
    Tablet,
    TV
}

public static class SizeManager
{
    private const string SizeKey = "HassWebView.ComponentSize";
    
    public static ComponentSize CurrentSize { get; private set; } = ComponentSize.Phone;
    
    public static event EventHandler? SizeChanged;
    
    /// <summary>
    /// 初始化尺寸管理器，优先从存储加载，否则使用默认值
    /// </summary>
    public static void Initialize(ComponentSize defaultSize = ComponentSize.Phone)
    {
        if (Preferences.ContainsKey(SizeKey))
        {
            var saved = Preferences.Get(SizeKey, "");
            if (Enum.TryParse<ComponentSize>(saved, out var size))
            {
                CurrentSize = size;
                UpdateSizeResources();
                return;
            }
        }
        CurrentSize = defaultSize;
        UpdateSizeResources();
    }
    
    public static void SetSize(ComponentSize size)
    {
        if (CurrentSize == size) return;
        
        CurrentSize = size;
        Preferences.Set(SizeKey, size.ToString());
        UpdateSizeResources();
        SizeChanged?.Invoke(null, EventArgs.Empty);
    }
    
    /// <summary>
    /// 更新资源字典中的尺寸别名，使其指向当前尺寸对应的具体值
    /// </summary>
    private static void UpdateSizeResources()
    {
        try
        {
            var resources = Application.Current?.Resources;
            if (resources == null) return;
            
            var sizePrefix = CurrentSize switch
            {
                ComponentSize.Phone => "Phone",
                ComponentSize.Tablet => "Tablet",
                ComponentSize.TV => "TV",
                _ => "Phone"
            };
            
            // 更新图标尺寸别名
            UpdateResourceAlias(resources, "ComponentIconSizeSmall", $"ComponentIconSize{sizePrefix}Small");
            UpdateResourceAlias(resources, "ComponentIconSizeMedium", $"ComponentIconSize{sizePrefix}Medium");
            UpdateResourceAlias(resources, "ComponentIconSizeLarge", $"ComponentIconSize{sizePrefix}Large");
            UpdateResourceAlias(resources, "ComponentIconSizeExtraLarge", $"ComponentIconSize{sizePrefix}ExtraLarge");
            
            // 更新标题字体尺寸别名
            UpdateResourceAlias(resources, "ComponentTitleSizeSmall", $"ComponentTitleSize{sizePrefix}Small");
            UpdateResourceAlias(resources, "ComponentTitleSizeMedium", $"ComponentTitleSize{sizePrefix}Medium");
            UpdateResourceAlias(resources, "ComponentTitleSizeLarge", $"ComponentTitleSize{sizePrefix}Large");
            UpdateResourceAlias(resources, "ComponentTitleSizeExtraLarge", $"ComponentTitleSize{sizePrefix}ExtraLarge");
            
            // 更新正文字体尺寸别名
            UpdateResourceAlias(resources, "ComponentBodySizeSmall", $"ComponentBodySize{sizePrefix}Small");
            UpdateResourceAlias(resources, "ComponentBodySizeMedium", $"ComponentBodySize{sizePrefix}Medium");
            UpdateResourceAlias(resources, "ComponentBodySizeLarge", $"ComponentBodySize{sizePrefix}Large");
            UpdateResourceAlias(resources, "ComponentBodySizeExtraLarge", $"ComponentBodySize{sizePrefix}ExtraLarge");
            
            // 更新内边距别名
            UpdateResourceAlias(resources, "ComponentPaddingSmall", $"ComponentPadding{sizePrefix}Small");
            UpdateResourceAlias(resources, "ComponentPaddingMedium", $"ComponentPadding{sizePrefix}Medium");
            UpdateResourceAlias(resources, "ComponentPaddingLarge", $"ComponentPadding{sizePrefix}Large");
            UpdateResourceAlias(resources, "ComponentPaddingExtraLarge", $"ComponentPadding{sizePrefix}ExtraLarge");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SizeManager.UpdateSizeResources failed: {ex}");
        }
    }
    
    /// <summary>
    /// 更新资源别名，使其指向目标资源的值
    /// </summary>
    private static void UpdateResourceAlias(ResourceDictionary resources, string aliasKey, string targetKey)
    {
        if (resources.ContainsKey(targetKey))
        {
            var targetValue = resources[targetKey];
            if (resources.ContainsKey(aliasKey))
            {
                resources[aliasKey] = targetValue;
            }
            else
            {
                resources.Add(aliasKey, targetValue);
            }
        }
    }
}
