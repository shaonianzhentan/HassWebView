namespace HassWebView.Component.Components;

using HassWebView.Component.Models;

public partial class SizeSelector : ContentView
{
    public SizeSelector()
    {
        InitializeComponent();
        UpdateButtonStates();
        SizeManager.SizeChanged += (s, e) => UpdateButtonStates();
    }

    private void OnSizeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string sizeStr)
        {
            if (Enum.TryParse<ComponentSize>(sizeStr, out var size))
            {
                SizeManager.SetSize(size);
            }
        }
    }

    private void UpdateButtonStates()
    {
        UpdateButton(MediumBtn, ComponentSize.Phone);
        UpdateButton(LargeBtn, ComponentSize.Tablet);
        UpdateButton(ExtraLargeBtn, ComponentSize.TV);
    }

    private void UpdateButton(Button btn, ComponentSize size)
    {
        bool isSelected = SizeManager.CurrentSize == size;
        btn.BackgroundColor = isSelected ? Color.FromHex("#007AFF") : Color.FromHex("#EFEFF4");
        btn.TextColor = isSelected ? Colors.White : Color.FromHex("#1D1D1F");
    }
}