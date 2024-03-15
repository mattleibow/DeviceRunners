using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp;

public partial class MainPage : ContentPage
{
	public MainPage(CounterViewModel counterViewModel)
	{
		Counter = counterViewModel;

		InitializeComponent();

		BindingContext = this;
	}

	public CounterViewModel Counter { get; }
}
