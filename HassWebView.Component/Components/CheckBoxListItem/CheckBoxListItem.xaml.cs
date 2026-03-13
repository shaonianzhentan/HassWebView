using System.ComponentModel;
using System.Windows.Input;
using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class CheckBoxListItem : ContentView
{
    public CheckBoxListItem()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 当复选框状态改变时触发的事件。
    /// </summary>
    public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

    #region Bindable Properties

    /// <summary>
    /// Controls the size of the component, affecting fonts and padding.
    /// </summary>
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(CheckBoxListItem), ComponentSize.Medium);

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// 列表项的主标题。
    /// </summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CheckBoxListItem), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 列表项的描述文本。
    /// </summary>
    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(CheckBoxListItem), string.Empty);

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// 复选框的选中状态。
    /// </summary>
    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CheckBoxListItem), false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// 当列表项被点击时执行的命令。
    /// </summary>
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CheckBoxListItem), null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// 为 Command 提供的参数。
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CheckBoxListItem), null);

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// CheckBox 的状态变化事件处理器。
    /// </summary>
    private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        // 更新 IsChecked 属性，并通过双向绑定通知 ViewModel
        IsChecked = e.Value;
        // 触发外部事件
        CheckedChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 整个项的点击事件处理器，用于切换复选框状态并执行命令。
    /// </summary>
    private void OnTapped(object sender, TappedEventArgs e)
    {
        // 切换状态
        IsChecked = !IsChecked;
        
        // 如果有命令，则执行
        if (Command?.CanExecute(CommandParameter) ?? false)
        {
            Command.Execute(CommandParameter);
        }
    }

    #endregion
}
