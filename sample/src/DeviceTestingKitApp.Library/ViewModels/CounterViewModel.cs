using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeviceTestingKitApp.ViewModels;

public partial class CounterViewModel : BaseViewModel
{
	[ObservableProperty]
	private int _count;

	[RelayCommand]
	private void Increment()
	{
		Count++;
	}
}
