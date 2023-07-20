namespace DeviceRunners.VisualRunners.Maui.Pages;

[QueryProperty(nameof(ViewModel), nameof(ViewModel))]
partial class TestResultPage : ContentPage
{
	public TestResultPage()
	{
		InitializeComponent();
	}

	public TestResultPage(TestResultViewModel testResultViewModel)
		: this()
	{
		ViewModel = testResultViewModel;
	}

	public TestResultViewModel? ViewModel
	{
		get => BindingContext as TestResultViewModel;
		set => BindingContext = value;
	}
}
