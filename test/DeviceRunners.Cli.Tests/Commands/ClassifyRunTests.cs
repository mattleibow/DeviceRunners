using DeviceRunners.Cli.Commands;

using Xunit;

using TestRunOutcome = DeviceRunners.Cli.Commands.BaseTestCommand<DeviceRunners.Cli.Commands.WindowsTestCommand.Settings>.TestRunOutcome;

namespace DeviceRunners.Cli.Tests;

public class ClassifyRunTests
{
	static TestRunOutcome Classify(bool hasStarted, bool hasEnded, int totalCount) =>
		BaseTestCommand<WindowsTestCommand.Settings>.ClassifyRun(hasStarted, hasEnded, totalCount);

	[Fact]
	public void NormalRunIsCompleted()
	{
		Assert.Equal(TestRunOutcome.Completed, Classify(hasStarted: true, hasEnded: true, totalCount: 5));
	}

	[Fact]
	public void ZeroMatchFilterIsCleanEmpty()
	{
		// begin + end received, but no results: a successful empty run.
		Assert.Equal(TestRunOutcome.CleanEmpty, Classify(hasStarted: true, hasEnded: true, totalCount: 0));
	}

	[Fact]
	public void BeginWithResultsButNoEndIsCrash()
	{
		Assert.Equal(TestRunOutcome.Crashed, Classify(hasStarted: true, hasEnded: false, totalCount: 3));
	}

	[Fact]
	public void NoConnectionIsNoResults()
	{
		Assert.Equal(TestRunOutcome.NoResults, Classify(hasStarted: false, hasEnded: false, totalCount: 0));
	}

	[Fact]
	public void BeginWithoutEndAndNoResultsIsNoResults()
	{
		// App connected and sent begin but never produced results or an end event
		// (e.g. it died immediately). Not a clean empty run, so it's a failure.
		Assert.Equal(TestRunOutcome.NoResults, Classify(hasStarted: true, hasEnded: false, totalCount: 0));
	}
}
