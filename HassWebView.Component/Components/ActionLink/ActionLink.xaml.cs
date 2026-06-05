namespace HassWebView.Component.Components;

using HassWebView.Component.Components.Base;
using System;

public partial class ActionLink : AdaptiveComponent
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ActionLink), string.Empty);

    public ActionLink()
    {
        InitializeComponent();
        LinkButton.Clicked += OnLinkButtonClicked;
        
        // 监听组件卸载事件
        Unloaded += OnUnloaded;
    }
    
    private void OnUnloaded(object? sender, EventArgs e)
    {
        LinkButton.Clicked -= OnLinkButtonClicked;
        Unloaded -= OnUnloaded;
    }

    private void OnLinkButtonClicked(object? sender, EventArgs e)
    {
        Clicked?.Invoke(this, e);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler? Clicked;
}