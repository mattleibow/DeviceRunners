using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.Library.MSTest3Tests.ViewModels;

[TestClass]
public class CounterViewModelTests
{
	[TestMethod]
	public void InitialStateIsCorrect()
	{
		var vm = new CounterViewModel();

		Assert.AreEqual(0, vm.Count);
	}

	[TestMethod]
	public void SingleIncrementIncrementsByOne()
	{
		var vm = new CounterViewModel();

		vm.IncrementCommand.Execute(null);

		Assert.AreEqual(1, vm.Count);
	}

	[TestMethod]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(3)]
	public void IncrementsIncrementCorrectly(int executeCount)
	{
		var vm = new CounterViewModel();

		for (var i = 0; i < executeCount; i++)
		{
			vm.IncrementCommand.Execute(null);
		}

		Assert.AreEqual(executeCount, vm.Count);
	}

	[TestMethod]
	public void UpdatingCountPropertyTriggersPropertyChanged()
	{
		var vm = new CounterViewModel();

		var eventTrigger = 0;
		vm.PropertyChanged += (sender, args) =>
		{
			Assert.AreEqual(nameof(vm.Count), args.PropertyName);
			eventTrigger++;
		};

		vm.Count = 5;

		Assert.AreEqual(1, eventTrigger);
	}

	[TestMethod]
	public void ExecutingCommandTriggersPropertyChanged()
	{
		var vm = new CounterViewModel();

		var eventTrigger = 0;
		vm.PropertyChanged += (sender, args) =>
		{
			Assert.AreEqual(nameof(vm.Count), args.PropertyName);
			eventTrigger++;
		};

		vm.IncrementCommand.Execute(null);

		Assert.AreEqual(1, eventTrigger);
	}
}
