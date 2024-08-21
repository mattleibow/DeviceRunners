using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.Library.NUnitTests.ViewModels;

public class CounterViewModelTests
{
	[Test]
	public void InitialStateIsCorrect()
	{
		var vm = new CounterViewModel();

		Assert.That(vm.Count, Is.EqualTo(0));
	}

	[Test]
	public void SingleIncrementIncrementsByOne()
	{
		var vm = new CounterViewModel();

		vm.IncrementCommand.Execute(null);

		Assert.That(vm.Count, Is.EqualTo(1));
	}

	[Test]
	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	public void IncrementsIncrementCorrectly(int executeCount)
	{
		var vm = new CounterViewModel();

		for (var i = 0; i < executeCount; i++)
		{
			vm.IncrementCommand.Execute(null);
		}

		Assert.That(vm.Count, Is.EqualTo(executeCount));
	}

	[Test]
	public void UpdatingCountPropertyTriggersPropertyChanged()
	{
		var vm = new CounterViewModel();

		var eventTrigger = 0;
		vm.PropertyChanged += (sender, args) =>
		{
			Assert.That(args.PropertyName, Is.EqualTo(nameof(vm.Count)));
			eventTrigger++;
		};

		vm.Count = 5;

		Assert.That(eventTrigger, Is.EqualTo(1));
	}

	[Test]
	public void ExecutingCommandTriggersPropertyChanged()
	{
		var vm = new CounterViewModel();

		var eventTrigger = 0;
		vm.PropertyChanged += (sender, args) =>
		{
			Assert.That(args.PropertyName, Is.EqualTo(nameof(vm.Count)));
			eventTrigger++;
		};

		vm.IncrementCommand.Execute(null);

		Assert.That(eventTrigger, Is.EqualTo(1));
	}
}
