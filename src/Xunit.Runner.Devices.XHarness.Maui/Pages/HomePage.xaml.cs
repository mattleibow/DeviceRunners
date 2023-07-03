namespace Xunit.Runner.Devices.XHarness.Maui.Pages;

partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

	public HomePage(HomeViewModel homeViewModel)
		: this()
	{
		ViewModel = homeViewModel;
	}

	public HomeViewModel? ViewModel
	{
		get => BindingContext as HomeViewModel;
		set
		{
			BindingContext = value;
		}
	}
}
