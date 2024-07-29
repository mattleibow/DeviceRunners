using System.Diagnostics;

using DeviceRunners.Core;

using Microsoft.DotNet.XHarness.DefaultAndroidEntryPoint.Xunit;
using Microsoft.DotNet.XHarness.TestRunners.Common;

namespace DeviceRunners.XHarness.Xunit;

public class XunitTestRunner : DefaultAndroidEntryPoint, ITestRunner
{
	readonly IXHarnessTestRunnerConfiguration _configuration;
	readonly IAppTerminator _appTerminator;

	public XunitTestRunner(IXHarnessTestRunnerConfiguration configuration, IDevice device, IAppTerminator appTerminator)
		: base(Path.Combine(Path.GetTempPath(), typeof(XunitTestRunner).FullName!, Guid.NewGuid().ToString()), new())
	{
		_configuration = configuration;
		_appTerminator = appTerminator;
		Device = device;
	}

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

		using var logWriter = CreateLogFile();

		TestsStarted += OnTestsStarted;
		TestStarted += OnTestStarted;
		TestCompleted += OnTestCompleted;
		TestsCompleted += OnTestsCompleted;

		await Task.Run(RunAsync);

		TestsCompleted -= OnTestsCompleted;
		TestCompleted -= OnTestCompleted;
		TestStarted -= OnTestStarted;
		TestsStarted -= OnTestsStarted;

		if (File.Exists(TestsResultsFinalPath))
			runResult["test-results-path"] = TestsResultsFinalPath;

		// make sure we mark this as an error if something crashed
		if (!runResult.ContainsKey("return-code"))
			runResult["return-code"] = "1";

		CopyFile(runResult);

		TerminateWithSuccess();

		return runResult;

		void OnTestsStarted(object? sender, EventArgs e) =>
			WriteLine("Test run started...");

		void OnTestStarted(object? sender, string test) =>
			WriteLine($"Test started: {test}");

		void OnTestCompleted(object? sender, (string TestName, TestResult TestResult) e) =>
			WriteLine($"Test completed: {e.TestResult.ToString().ToUpperInvariant()} {e.TestName}");

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

			WriteLine("Test run completed.");
			WriteLine(message);
		}

		void WriteLine(string line)
		{
			Debug.WriteLine(line);

			logWriter?.WriteLine($"[{DateTime.Now:hh:mm:ss}] {line}");
			logWriter?.Flush();
		}
	}

	void CopyFile(ITestRunResult result)
	{
		var resultsFile = result["test-results-path"];
		if (resultsFile is null)
			return;

		if (_configuration.OutputDirectory is null)
			return;

		Directory.CreateDirectory(_configuration.OutputDirectory);

		var filename = Path.GetFileName(resultsFile);
		var destination = Path.Combine(_configuration.OutputDirectory, filename);

		File.Copy(resultsFile, destination, true);

		result["test-results-path"] = destination;
	}

	TextWriter? CreateLogFile()
	{
		if (_configuration.OutputDirectory is null)
			return null;

		Directory.CreateDirectory(_configuration.OutputDirectory);

		var filename = Path.GetFileName($"test-output-{DateTime.Now:yyyyMMdd_hhmmss}.log");
		var destination = Path.Combine(_configuration.OutputDirectory, filename);

		return new StreamWriter(destination);
	}
}
