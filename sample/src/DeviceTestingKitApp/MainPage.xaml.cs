using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel mainViewModel)
	{
		BindingContext = mainViewModel;

		InitializeComponent();
	}
}
