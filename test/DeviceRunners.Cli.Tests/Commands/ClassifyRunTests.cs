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

	static bool IsSuccess(TestRunOutcome outcome, int failedCount) =>
		BaseTestCommand<WindowsTestCommand.Settings>.OutcomeIsSuccess(outcome, failedCount);

	static int ExitCode(TestRunOutcome outcome, int failedCount) =>
		BaseTestCommand<WindowsTestCommand.Settings>.OutcomeToExitCode(outcome, failedCount);

	[Fact]
	public void CompletedRunSucceedsOnlyWithNoFailures()
	{
		Assert.True(IsSuccess(TestRunOutcome.Completed, failedCount: 0));
		Assert.Equal(0, ExitCode(TestRunOutcome.Completed, failedCount: 0));

		Assert.False(IsSuccess(TestRunOutcome.Completed, failedCount: 2));
		Assert.Equal(1, ExitCode(TestRunOutcome.Completed, failedCount: 2));
	}

	[Fact]
	public void CleanEmptyRunSucceeds()
	{
		Assert.True(IsSuccess(TestRunOutcome.CleanEmpty, failedCount: 0));
		Assert.Equal(0, ExitCode(TestRunOutcome.CleanEmpty, failedCount: 0));
	}

	[Fact]
	public void CrashIsNeverSuccessEvenWithNoRecordedFailures()
	{
		// Regression guard: a crash/timeout mid-run (begin + partial results, no end)
		// must not be reported as a successful, passing run just because none of the
		// results that did arrive were failures.
		Assert.False(IsSuccess(TestRunOutcome.Crashed, failedCount: 0));
		Assert.Equal(2, ExitCode(TestRunOutcome.Crashed, failedCount: 0));
	}

	[Fact]
	public void NoResultsIsFailure()
	{
		Assert.False(IsSuccess(TestRunOutcome.NoResults, failedCount: 0));
		Assert.Equal(1, ExitCode(TestRunOutcome.NoResults, failedCount: 0));
	}
}
