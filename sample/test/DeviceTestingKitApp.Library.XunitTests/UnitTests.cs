using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.Library.XunitTests;

public class UnitTests
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

	[Fact(Skip = "This test is skipped.")]
	public void SkippedTest()
	{
	}

#if INCLUDE_FAILING_TESTS
	[Fact]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
#endif
}
