using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;

using ObjCRuntime;

using UIKit;

namespace Xunit.Runner.Devices.XHarness.Maui;

public class XHarnessRunner : iOSApplicationEntryPoint
{
	readonly RunnerOptions _options;
	readonly ApplicationOptions _applicationOptions;

	public XHarnessRunner(RunnerOptions options, ApplicationOptions applicationOptions, IDevice device)
	{
		_options = options;
		_applicationOptions = applicationOptions;
		Device = device;
	}

	protected override bool LogExcludedTests => true;

	protected override int? MaxParallelThreads => Environment.ProcessorCount;

	protected override IDevice Device { get; }

	protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
		_options.Assemblies
			.Distinct()
			.Select(assembly => new TestAssemblyInfo(assembly, assembly.Location));

	protected override void TerminateWithSuccess()
	{
		var s = new ObjCRuntime.Selector("terminateWithSuccess");
		UIApplication.SharedApplication.PerformSelector(s, UIApplication.SharedApplication, 0);
	}

	protected override TestRunner GetTestRunner(LogWriter logWriter)
	{
		var testRunner = base.GetTestRunner(logWriter);

		testRunner.RunInParallel = true;

		if (_options.SkipCategories?.Count > 0)
			testRunner.SkipCategories(_options.SkipCategories);

		return testRunner;
	}

	public async Task RunTestsAsync()
	{
		TestsCompleted += OnTestsCompleted;

		if (_applicationOptions.AutoStart || _applicationOptions.TerminateAfterExecution)
			await Task.Run(RunAsync);

		TestsCompleted -= OnTestsCompleted;

		if (_applicationOptions.TerminateAfterExecution)
			TerminateWithSuccess();

		static void OnTestsCompleted(object? sender, TestRunResult results)
		{
			var message =
				$"Tests run: {results.ExecutedTests} " +
				$"Passed: {results.PassedTests} " +
				$"Inconclusive: {results.InconclusiveTests} " +
				$"Failed: {results.FailedTests} " +
				$"Ignored: {results.SkippedTests}";

			Console.WriteLine(message);
		}
	}
}
