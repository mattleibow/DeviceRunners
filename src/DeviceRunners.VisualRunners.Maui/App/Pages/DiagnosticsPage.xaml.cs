namespace DeviceRunners.VisualRunners.Maui.Pages;

partial class DiagnosticsPage : ContentPage
{
	public DiagnosticsPage()
	{
		InitializeComponent();
	}

	public DiagnosticsPage(DiagnosticsViewModel diagnosticsViewModel)
		: this()
	{
		ViewModel = diagnosticsViewModel;
	}

	public DiagnosticsViewModel? ViewModel
	{
		get => BindingContext as DiagnosticsViewModel;
		set => BindingContext = value;
	}
}
