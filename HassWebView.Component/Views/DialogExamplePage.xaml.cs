namespace HassWebView.Component.Views;

public partial class DialogExamplePage : ContentPage
{
	public DialogExamplePage()
	{
		InitializeComponent();
	}

    private void OnShowDialogClicked(object sender, EventArgs e)
    {
        ExampleDialog.Show();
    }

    private void OnDialogConfirmClicked(object sender, EventArgs e)
    {
        ExampleDialog.Hide();
    }
}