namespace DeviceRunners.XHarness.Maui.Pages;

partial class HomePage : ContentPage
{
	bool first = true;
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
		set => BindingContext = value;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (!first)
			return;

		first = false;

		await ViewModel.RunTestsAsync();
	}
}
