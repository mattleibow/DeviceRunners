namespace DeviceRunners.VisualRunners.Maui.Pages;

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
			if (ViewModel is HomeViewModel vm)
				vm.TestAssemblySelected -= OnTestAssemblySelected;

			BindingContext = value;

			if (value is not null)
				value.TestAssemblySelected += OnTestAssemblySelected;
		}
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		assemblyList.SelectedItem = null;

		if (ViewModel is HomeViewModel vm)
			await vm.StartAssemblyScanAsync();
	}

	async void OnTestAssemblySelected(object? sender, TestAssemblyViewModel testAssemblyViewModel)
	{
		await Shell.Current.GoToAsync($"assembly", new Dictionary<string, object>
		{
			[nameof(TestAssemblyPage.ViewModel)] = testAssemblyViewModel
		});
	}
}
