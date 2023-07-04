using Android.App;
using Android.OS;

using Microsoft.DotNet.XHarness.DefaultAndroidEntryPoint.Xunit;
using Microsoft.DotNet.XHarness.TestRunners.Common;

namespace Xunit.Runner.Devices.XHarness;

public class XHarnessRunner : DefaultAndroidEntryPoint, ITestRunner
{
	readonly RunnerOptions _options;
	readonly ApplicationOptions _applicationOptions;

	public XHarnessRunner(RunnerOptions options, ApplicationOptions applicationOptions, IDevice device)
		: base(Android.App.Application.Context.CacheDir!.AbsolutePath, new())
	{
		_options = options;
		_applicationOptions = applicationOptions;
		Device = device;
	}

	protected override IDevice Device { get; }

	protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
		_options.Assemblies
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

		if (_options.SkipCategories?.Count > 0)
			testRunner.SkipCategories(_options.SkipCategories);

		return testRunner;
	}

	public async Task<Bundle> RunTestsAsync()
	{
		var bundle = new Bundle();

		TestsCompleted += OnTestsCompleted;

		await Task.Run(RunAsync);

		TestsCompleted -= OnTestsCompleted;

		if (File.Exists(TestsResultsFinalPath))
			bundle.PutString("test-results-path", TestsResultsFinalPath);

		if (bundle.GetLong("return-code", -1) == -1)
			bundle.PutLong("return-code", 1);

		return bundle;

		void OnTestsCompleted(object? sender, TestRunResult results)
		{
			var message =
				$"Tests run: {results.ExecutedTests} " +
				$"Passed: {results.PassedTests} " +
				$"Inconclusive: {results.InconclusiveTests} " +
				$"Failed: {results.FailedTests} " +
				$"Ignored: {results.SkippedTests}";

			bundle.PutString("test-execution-summary", message);

			bundle.PutLong("return-code", results.FailedTests == 0 ? 0 : 1);
		}
	}

	async Task<object> ITestRunner.RunTestsAsync()
	{
		var bundle = await RunTestsAsync();
		return bundle;
	}
}
