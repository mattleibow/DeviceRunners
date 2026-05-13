using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Controls;

public class CounterViewModelTests
{
	[Fact]
	public void CounterViewModel_InitialCount_IsZero()
	{
		var vm = new CounterViewModel();
		Assert.Equal(0, vm.Count);
	}

	[Fact]
	public void CounterViewModel_Increment_IncreasesCount()
	{
		var vm = new CounterViewModel();
		vm.IncrementCommand.Execute(null);
		Assert.Equal(1, vm.Count);
	}
}
