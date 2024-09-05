namespace DeviceRunners.UIAutomation.Selenium;

public static partial class SeleniumAutomatedAppOptionsBuilderExtensions
{
	private const string DriverOptionPrefix = SeleniumAutomatedAppOptionsExtensions.DriverOptionPrefix;
	private const string InitialUrlDriverOption = SeleniumAutomatedAppOptionsExtensions.InitialUrlDriverOption;

	private static TBuilder AddAdditionalOption<TBuilder>(this TBuilder builder, string name, object value)
		where TBuilder : SeleniumAutomatedAppOptionsBuilder
	{
		builder.DriverOptions.AddAdditionalOption(DriverOptionPrefix + name, value);
		return builder;
	}

	public static TBuilder UseInitialUrl<TBuilder>(this TBuilder builder, string initialUrl)
		where TBuilder : SeleniumAutomatedAppOptionsBuilder
	{
		builder.AddAdditionalOption(InitialUrlDriverOption, initialUrl);
		return builder;
	}
}
