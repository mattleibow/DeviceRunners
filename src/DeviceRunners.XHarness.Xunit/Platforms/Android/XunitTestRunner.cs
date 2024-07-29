using Microsoft.DotNet.XHarness.DefaultAndroidEntryPoint.Xunit;
using Microsoft.DotNet.XHarness.TestRunners.Common;

namespace DeviceRunners.XHarness.Xunit;

public class XunitTestRunner : DefaultAndroidEntryPoint, ITestRunner
{
	readonly IXHarnessTestRunnerConfiguration _configuration;

	public XunitTestRunner(IXHarnessTestRunnerConfiguration configuration, IDevice device)
		: base(Application.Context.CacheDir!.AbsolutePath, new())
	{
		_configuration = configuration;
		Device = device;
	}

	protected override IDevice Device { get; }

	protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
		_configuration.TestAssemblies
			.Distinct()
			.Select(assembly =>
			{
				// Android needs this file to "exist" but it uses the assembly actually.
				var path = Path.Combine(Application.Context.CacheDir!.AbsolutePath, assembly.GetName().Name + ".dll");
				if (!File.Exists(path))
					File.Create(path).Close();

				return new TestAssemblyInfo(assembly, path);
			});

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

		if (File.Exists(TestsResultsFinalPath))
			runResult["test-results-path"] = TestsResultsFinalPath;

		// make sure we mark this as an error if something crashed
		if (!runResult.ContainsKey("return-code"))
			runResult["return-code"] = "1";

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
