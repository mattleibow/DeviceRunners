using DeviceRunners.Cli.Commands;

namespace DeviceRunners.Cli.Tests;

public class AppEnvironmentVariableTests
{
	[Fact]
	public void Filter_WhenSet_IsAddedToEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			Filter = "FullyQualifiedName~MyClass",
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.Equal("FullyQualifiedName~MyClass", env["DEVICE_RUNNERS_FILTER"]);
		Assert.Equal("1", env["DEVICE_RUNNERS_AUTORUN"]);
	}

	[Fact]
	public void Filter_WhenNotSet_IsOmittedFromEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.False(env.ContainsKey("DEVICE_RUNNERS_FILTER"));
	}

	[Fact]
	public void Filter_WhenWhitespace_IsOmittedFromEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			Filter = "   ",
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.False(env.ContainsKey("DEVICE_RUNNERS_FILTER"));
	}

	[Fact]
	public void SimpleFilters_AreTranslatedIntoEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			FilterClass = new[] { "Calc*" },
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.Equal("ClassName=Calc*", env["DEVICE_RUNNERS_FILTER"]);
	}

	[Fact]
	public void SimpleFilters_MultipleKinds_AreCombinedInEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			FilterClass = new[] { "A" },
			FilterTrait = new[] { "Cat=Fast" },
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.Equal("ClassName=A & Cat=Fast", env["DEVICE_RUNNERS_FILTER"]);
	}

	[Fact]
	public void GetEffectiveFilter_PrefersSimpleFilters_OverRawFilter()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			FilterClass = new[] { "Calc" },
		};

		Assert.Equal("ClassName=Calc", BaseTestCommand<WindowsTestCommand.Settings>.GetEffectiveFilter(settings));
	}

	[Fact]
	public void GetEffectiveFilter_FallsBackToRawFilter_WhenNoSimpleFilters()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			Filter = "FullyQualifiedName~MyClass",
		};

		Assert.Equal("FullyQualifiedName~MyClass", BaseTestCommand<WindowsTestCommand.Settings>.GetEffectiveFilter(settings));
	}

	[Fact]
	public void Validate_RejectsMixingRawAndSimpleFilters()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			Filter = "FullyQualifiedName~MyClass",
			FilterClass = new[] { "Calc" },
		};

		Assert.False(settings.Validate().Successful);
	}

	[Fact]
	public void Validate_AllowsSimpleFiltersWithoutRawFilter()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			FilterClass = new[] { "Calc" },
		};

		Assert.True(settings.Validate().Successful);
	}
}
