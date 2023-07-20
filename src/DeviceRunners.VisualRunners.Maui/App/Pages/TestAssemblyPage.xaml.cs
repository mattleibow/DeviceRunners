namespace DeviceRunners.VisualRunners.Maui.Pages;

[QueryProperty(nameof(ViewModel), nameof(ViewModel))]
partial class TestAssemblyPage : ContentPage
{
	public TestAssemblyPage()
	{
		InitializeComponent();
	}

	public TestAssemblyPage(TestAssemblyViewModel testAssemblyViewModel)
		: this()
	{
		ViewModel = testAssemblyViewModel;
	}

	public TestAssemblyViewModel? ViewModel
	{
		get => BindingContext as TestAssemblyViewModel;
		set => BindingContext = value;
	}

	protected override void OnDisappearing()
	{
		if (ViewModel is TestAssemblyViewModel vm)
			vm.TestResultSelected -= OnTestResultSelected;

		base.OnDisappearing();

		testsList.SelectedItem = null;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		testsList.SelectedItem = null;

		if (ViewModel is TestAssemblyViewModel vm)
			vm.TestResultSelected += OnTestResultSelected;
	}

	async void OnTestResultSelected(object? sender, TestResultViewModel testResultViewModel)
	{
		await Shell.Current.GoToAsync($"result", new Dictionary<string, object>
		{
			[nameof(TestResultPage.ViewModel)] = testResultViewModel
		});
	}
}
