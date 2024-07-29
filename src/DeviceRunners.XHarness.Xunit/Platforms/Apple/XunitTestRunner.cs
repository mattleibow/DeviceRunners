using DeviceRunners.Core;

using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;

namespace DeviceRunners.XHarness.Xunit;

public class XunitTestRunner : iOSApplicationEntryPoint, ITestRunner
{
	readonly IXHarnessTestRunnerConfiguration _configuration;
	readonly IAppTerminator _appTerminator;

	public XunitTestRunner(IXHarnessTestRunnerConfiguration configuration, IDevice device, IAppTerminator appTerminator)
	{
		_configuration = configuration;
		_appTerminator = appTerminator;
		Device = device;
	}

	protected override bool LogExcludedTests => true;

	protected override int? MaxParallelThreads => Environment.ProcessorCount;

	protected override IDevice Device { get; }

	protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
		_configuration.TestAssemblies
			.Distinct()
			.Select(assembly => new TestAssemblyInfo(assembly, assembly.Location));

	protected override void TerminateWithSuccess() =>
		_appTerminator.Terminate();

	protected override TestRunner GetTestRunner(LogWriter logWriter)
	{
		var testRunner = base.GetTestRunner(logWriter);

		testRunner.RunInParallel = true;

		if (_configuration.SkipCategories?.Count > 0)
			testRunner.SkipCategories(_configuration.SkipCategories);

		return testRunner;
	}

	public async Task<ITestRunResult> RunTestsAsync()
	{
		var runResult = new XHarnessTestRunResult();

		TestsCompleted += OnTestsCompleted;

		await Task.Run(RunAsync);

		TestsCompleted -= OnTestsCompleted;

		// make sure we mark this as an error if something crashed
		if (!runResult.ContainsKey("return-code"))
			runResult["return-code"] = "1";

		TerminateWithSuccess();

		return runResult;

		void OnTestsCompleted(object? sender, TestRunResult result)
		{
			var message =
				$"Tests run: {result.ExecutedTests} " +
				$"Passed: {result.PassedTests} " +
				$"Inconclusive: {result.InconclusiveTests} " +
				$"Failed: {result.FailedTests} " +
				$"Ignored: {result.SkippedTests}";

			runResult["test-execution-summary"] = message;

			runResult["return-code"] = result.FailedTests == 0 ? "0" : "1";

			Console.WriteLine(message);
		}
	}
}
