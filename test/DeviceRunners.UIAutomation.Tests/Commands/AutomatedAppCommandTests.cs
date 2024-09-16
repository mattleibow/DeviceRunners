using DeviceRunners.UIAutomation;

using NSubstitute;

using Xunit;

namespace UIAutomationTests.Commands;

public class AutomatedAppCommandTests
{
	[Fact]
	public void CommandWithExactAppTypeExecutes()
	{
		var command = Substitute.For<AutomatedAppCommand<TestApp>>("Test");
		command.Execute(Arg.Any<TestApp>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns("Success");

		var app = new TestApp([command]);

		Assert.Equal("Test", command.Name);

		var result = command.Execute(app);

		command.Received().Execute(app);
		Assert.Equal("Success", result);
	}

	[Fact]
	public void CommandWithDerivedAppTypeExecutes()
	{
		var command = Substitute.For<AutomatedAppCommand<TestApp>>("Test");
		command.Execute(Arg.Any<TestApp>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns("Success");

		Assert.Equal("Test", command.Name);

		var app = new DerivedApp([command]);
		var result = command.Execute(app);

		command.Received().Execute(app);
		Assert.Equal("Success", result);
	}

	[Fact]
	public void CommandWithAnotherAppTypeExecutes()
	{
		var command = Substitute.For<AutomatedAppCommand<TestApp>>("Test");
		command.Execute(Arg.Any<TestApp>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns("Success");

		Assert.Equal("Test", command.Name);

		var app = new AnotherApp([command]);

		Assert.Throws<ArgumentException>("app", () => ((IAutomatedAppCommand)command).Execute(app));
	}

	public class TestApp : IAutomatedApp
	{
		public TestApp(IReadOnlyList<IAutomatedAppCommand> commands)
		{
			Commands = new AutomatedAppCommandManager(this, commands);
		}

		public IAutomatedAppCommandManager Commands { get; }

		public IAutomatedAppElement FindElement(Action<IBy> by) =>
			throw new NotImplementedException();

		public IReadOnlyList<IAutomatedAppElement> FindElements(Action<IBy> by) =>
			throw new NotImplementedException();
	}

	public class DerivedApp : TestApp
	{
		public DerivedApp(IReadOnlyList<IAutomatedAppCommand> commands)
			: base(commands)
		{
		}
	}

	public class AnotherApp : IAutomatedApp
	{
		public AnotherApp(IReadOnlyList<IAutomatedAppCommand> commands)
		{
			Commands = new AutomatedAppCommandManager(this, commands);
		}

		public IAutomatedAppCommandManager Commands { get; }

		public IAutomatedAppElement FindElement(Action<IBy> by) =>
			throw new NotImplementedException();

		public IReadOnlyList<IAutomatedAppElement> FindElements(Action<IBy> by) =>
			throw new NotImplementedException();
	}
}
