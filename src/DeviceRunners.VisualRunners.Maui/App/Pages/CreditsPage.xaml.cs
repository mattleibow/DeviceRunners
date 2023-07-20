namespace DeviceRunners.VisualRunners.Maui.Pages;

partial class CreditsPage : ContentPage
{
	bool _initial = true;

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
		if (_initial)
		{
			_initial = false;
			return;
		}

		Browser.OpenAsync(e.Url);

		e.Cancel = true;
	}
}
