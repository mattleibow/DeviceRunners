namespace DeviceRunners.UIAutomation;

public static class AutomatedAppOptionsBuilderExtensions
{
	public static TBuilder AddCommand<TBuilder>(this TBuilder builder, IAutomatedAppCommand command)
		where TBuilder : IAutomatedAppOptionsBuilder
	{
		builder.AddCommand(command);

		return builder;
	}
}
