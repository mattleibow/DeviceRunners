namespace Xunit.Runner.Devices.VisualRunner.Maui.Pages;

partial class CreditsPage : ContentPage
{
	bool initial = true;

	public CreditsPage()
	{
		InitializeComponent();
	}

	public CreditsPage(CreditsViewModel creditsViewModel)
		: this()
	{
		ViewModel = creditsViewModel;
	}

	public CreditsViewModel? ViewModel
	{
		get => BindingContext as CreditsViewModel;
		set => BindingContext = value;
	}

	void OnNavigating(object? sender, WebNavigatingEventArgs e)
	{
		if (initial)
		{
			initial = false;
			return;
		}

		Browser.OpenAsync(e.Url);

		e.Cancel = true;
	}
}
