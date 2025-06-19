using DeviceRunners.UIAutomation;

using NSubstitute;

using Xunit;

namespace UIAutomationTests.Commands;

public class AutomatedAppOptionsBuilderExtensions
{
	[Fact]
	public void AddCommandAddsTheCommand()
	{
		var command = Substitute.For<IAutomatedAppCommand>();

		var builder = new TestOptionsBuilder();
		builder.AddCommand(command);

		Assert.Contains(command, builder.Commands);
	}

	class TestOptionsBuilder : IAutomatedAppOptionsBuilder
	{
		public List<IAutomatedAppCommand> Commands { get; } = [];

		void IAutomatedAppOptionsBuilder.AddCommand(IAutomatedAppCommand command) => Commands.Add(command);

		public IAutomatedAppOptions Build() => throw new NotImplementedException();
	}
}
