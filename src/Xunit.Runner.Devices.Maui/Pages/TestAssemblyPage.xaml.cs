namespace Xunit.Runner.Devices.Maui.Pages;

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
		set
		{
			if (ViewModel is TestAssemblyViewModel vm)
				vm.TestResultSelected -= OnTestResultSelected;

			BindingContext = value;

			if (value is not null)
				value.TestResultSelected += OnTestResultSelected;
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		testsList.SelectedItem = null;
	}

	async void OnTestResultSelected(object? sender, TestResultViewModel testResultViewModel)
	{
		await Shell.Current.GoToAsync($"//runner/assembly/result", new Dictionary<string, object>
		{
			[nameof(TestResultPage.ViewModel)] = testResultViewModel
		});
	}
}
