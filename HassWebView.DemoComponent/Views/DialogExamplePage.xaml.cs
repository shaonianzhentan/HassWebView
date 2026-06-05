namespace HassWebView.DemoComponent.Views;

using HassWebView.Component.Components;

public partial class DialogExamplePage : ContentPage
{
	public DialogExamplePage()
	{
		InitializeComponent();
	}

    private void OnShowDialogClicked(object? sender, EventArgs e)
    {
        ExampleDialog.Show();
    }

    private void OnShowDialogNoTitleClicked(object? sender, EventArgs e)
    {
        NoTitleDialog.Show();
    }

    private void OnShowCustomDialogClicked(object? sender, EventArgs e)
    {
        CustomDialog.Show();
    }

    private void OnDialogConfirmClicked(object? sender, EventArgs e)
    {
        if (sender is Dialog dialog)
        {
            dialog.Hide();
        }
    }
}