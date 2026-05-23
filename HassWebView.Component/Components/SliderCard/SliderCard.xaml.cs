using System.Windows.Input;
using HassWebView.Component.Models;

namespace HassWebView.Component;

public partial class SliderCard : ContentView
{
    public SliderCard()
    {
        InitializeComponent();
        UpdateValueLabel();
    }

    /// <summary>
    /// Fired when dragging ends and the final value is committed (step-snapped).
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    #region Bindable Properties

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SliderCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(SliderCard), 0.0,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (b, _, _) => ((SliderCard)b).UpdateValueLabel());

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MinProperty =
        BindableProperty.Create(nameof(Min), typeof(double), typeof(SliderCard), 0.0);

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public static readonly BindableProperty MaxProperty =
        BindableProperty.Create(nameof(Max), typeof(double), typeof(SliderCard), 100.0);

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>
    /// Step increment. After drag completes, the value is snapped to the nearest multiple.
    /// </summary>
    public static readonly BindableProperty StepProperty =
        BindableProperty.Create(nameof(Step), typeof(double), typeof(SliderCard), 1.0);

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Format string for the displayed value. e.g. "{0:0}", "{0:0.0}"
    /// </summary>
    public static readonly BindableProperty ValueFormatProperty =
        BindableProperty.Create(nameof(ValueFormat), typeof(string), typeof(SliderCard), "{0:0}",
            propertyChanged: (b, _, _) => ((SliderCard)b).UpdateValueLabel());

    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    /// <summary>
    /// Unit suffix appended after the formatted value. e.g. "%", "°C"
    /// </summary>
    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(SliderCard), string.Empty,
            propertyChanged: (b, _, _) => ((SliderCard)b).UpdateValueLabel());

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SliderCard), null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ComponentSize), typeof(SliderCard), ComponentSize.Medium);

    public ComponentSize Size
    {
        get => (ComponentSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    #endregion

    private void UpdateValueLabel()
    {
        try
        {
            ValueLabel.Text = string.Format(ValueFormat, Value) + Unit;
        }
        catch
        {
            ValueLabel.Text = $"{Value}{Unit}";
        }
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        // Update label while dragging (live preview), but don't fire ValueChanged yet
        UpdateValueLabel();
    }

    private void OnDragCompleted(object sender, EventArgs e)
    {
        // Snap to nearest Step multiple
        if (Step > 0)
        {
            double snapped = Math.Round(MainSlider.Value / Step) * Step;
            snapped = Math.Max(Min, Math.Min(Max, snapped));
            if (Math.Abs(Value - snapped) > 0.0001)
            {
                Value = snapped;
            }
        }

        UpdateValueLabel();

        var args = new ValueChangedEventArgs(0, Value);
        ValueChanged?.Invoke(this, args);
        if (Command?.CanExecute(Value) ?? false)
            Command.Execute(Value);
    }
}
