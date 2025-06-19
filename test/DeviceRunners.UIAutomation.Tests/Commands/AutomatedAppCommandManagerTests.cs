using DeviceRunners.UIAutomation;

using NSubstitute;

using Xunit;

namespace UIAutomationTests.Commands;

public class AutomatedAppCommandManagerTests
{
	[Fact]
	public void AppPropertyIsCorrect()
	{
		var app = Substitute.For<IAutomatedApp>();

		var manager = new AutomatedAppCommandManager(app, []);

		Assert.Equal(app, manager.App);
	}

	[Fact]
	public void CommandsAreAvailable()
	{
		var app = Substitute.For<IAutomatedApp>();

		var command = Substitute.For<IAutomatedAppCommand>();
		command.Name.Returns("Test");

		var manager = new AutomatedAppCommandManager(app, [command]);

		Assert.True(manager.ContainsCommand("Test"), "The 'Test' command was not found.");
		Assert.False(manager.ContainsCommand("Foo"), "The 'Foo' command was somehow found.");
	}

	[Fact]
	public void AvailableCommandsCanBeExecuted()
	{
		var app = Substitute.For<IAutomatedApp>();

		var command = Substitute.For<IAutomatedAppCommand>();
		command.Name.Returns("Test");
		command.Execute(Arg.Any<IAutomatedApp>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns("Success");

		var manager = new AutomatedAppCommandManager(app, [command]);

		var result = manager.Execute("Test");

		command.Received().Execute(app, null);

		Assert.Equal("Success", result);
	}

	[Fact]
	public void CommandParametersArePassedToTheCommand()
	{
		var app = Substitute.For<IAutomatedApp>();

		var param = new Dictionary<string, object>();

		var command = Substitute.For<IAutomatedAppCommand>();
		command.Name.Returns("Test");
		command.Execute(Arg.Any<IAutomatedApp>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns("Success");

		var manager = new AutomatedAppCommandManager(app, [command]);

		var result = manager.Execute("Test", param);

		command.Received().Execute(app, param);

		Assert.Equal("Success", result);
	}

	[Fact]
	public void UnavailableCommandsCannotBeExecuted()
	{
		var app = Substitute.For<IAutomatedApp>();

		var command = Substitute.For<IAutomatedAppCommand>();
		command.Name.Returns("Test");
		command.Execute(Arg.Any<IAutomatedApp>(), Arg.Any<IReadOnlyDictionary<string, object>>()).Returns(null);

		var manager = new AutomatedAppCommandManager(app, [command]);

		Assert.Throws<KeyNotFoundException>(() => manager.Execute("Foo"));
	}
}
