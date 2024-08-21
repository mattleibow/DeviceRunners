namespace DeviceTestingKitApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
	public MainViewModel(CounterViewModel counter)
	{
		Counter = counter;
	}

	public CounterViewModel Counter { get; }
}
