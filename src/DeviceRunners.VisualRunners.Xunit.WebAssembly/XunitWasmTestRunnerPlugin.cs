using System.Diagnostics;
using System.Reflection;

using DeviceRunners.VisualRunners.WebAssembly;

namespace DeviceRunners.VisualRunners.Xunit;

/// <summary>
/// Xunit test runner plugin for browser WASM environments.
/// Uses reflection to discover [Fact] and [Theory] tests and execute them directly,
/// bypassing XunitFrontController which requires filesystem access unavailable in WASM.
/// </summary>
public class XunitWasmTestRunnerPlugin : IWasmTestRunnerPlugin
{
	public async Task<WasmTestRunResult> RunTestsAsync(
		IEnumerable<Assembly> assemblies,
		IResultChannel resultChannel,
		CancellationToken cancellationToken = default)
	{
		var counts = new TestCounts();

		foreach (var assembly in assemblies)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			var assemblyFileName = assembly.GetName().Name + ".dll";
			var assemblyInfo = new WasmXunitAssemblyInfo(assemblyFileName);

			foreach (var type in assembly.GetExportedTypes())
			{
				if (type.IsAbstract || type.IsInterface)
					continue;

				foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
				{
					if (cancellationToken.IsCancellationRequested)
						break;

					var factAttr = method.GetCustomAttribute<global::Xunit.FactAttribute>();
					if (factAttr is null)
						continue;

					var skipReason = factAttr.Skip;

					var inlineDataAttrs = method.GetCustomAttributes<global::Xunit.InlineDataAttribute>().ToList();
					bool isTheory = inlineDataAttrs.Count > 0;

					if (isTheory)
					{
						foreach (var inlineData in inlineDataAttrs)
						{
							var data = inlineData.GetData(method).First();
							var displayName = $"{type.FullName}.{method.Name}({string.Join(", ", data.Select(d => d?.ToString() ?? "null"))})";
							var testCase = new WasmXunitTestCaseInfo(assemblyInfo, displayName);

							await RunSingleTestAsync(type, method, data, testCase, resultChannel, skipReason, counts);
						}
					}
					else
					{
						var displayName = $"{type.FullName}.{method.Name}";
						var testCase = new WasmXunitTestCaseInfo(assemblyInfo, displayName);

						await RunSingleTestAsync(type, method, null, testCase, resultChannel, skipReason, counts);
					}
				}
			}
		}

		return new WasmTestRunResult
		{
			TotalTests = counts.Total,
			PassedTests = counts.Passed,
			FailedTests = counts.Failed,
			SkippedTests = counts.Skipped
		};
	}

	static async Task RunSingleTestAsync(
		Type type, MethodInfo method, object?[]? args,
		WasmXunitTestCaseInfo testCase, IResultChannel resultChannel,
		string? skipReason, TestCounts counts)
	{
		counts.Total++;

		if (skipReason is not null)
		{
			counts.Skipped++;
			var skipResult = new WasmXunitTestResultInfo(testCase, TestResultStatus.Skipped, TimeSpan.Zero, null, null, null, skipReason);
			testCase.ReportResult(skipResult);
			resultChannel.RecordResult(skipResult);
			return;
		}

		var sw = Stopwatch.StartNew();
		try
		{
			var instance = Activator.CreateInstance(type)!;
			var result = method.Invoke(instance, args);

			// Handle async methods
			if (result is Task task)
				await task;

			sw.Stop();
			counts.Passed++;
			var passResult = new WasmXunitTestResultInfo(testCase, TestResultStatus.Passed, sw.Elapsed, null, null, null, null);
			testCase.ReportResult(passResult);
			resultChannel.RecordResult(passResult);

			// Dispose if needed
			if (instance is IDisposable disposable)
				disposable.Dispose();
			if (instance is IAsyncDisposable asyncDisposable)
				await asyncDisposable.DisposeAsync();
		}
		catch (TargetInvocationException tie)
		{
			sw.Stop();
			var ex = tie.InnerException ?? tie;
			counts.Failed++;
			var failResult = new WasmXunitTestResultInfo(testCase, TestResultStatus.Failed, sw.Elapsed, null, ex.Message, ex.StackTrace, null);
			testCase.ReportResult(failResult);
			resultChannel.RecordResult(failResult);
		}
		catch (Exception ex)
		{
			sw.Stop();
			counts.Failed++;
			var failResult = new WasmXunitTestResultInfo(testCase, TestResultStatus.Failed, sw.Elapsed, null, ex.Message, ex.StackTrace, null);
			testCase.ReportResult(failResult);
			resultChannel.RecordResult(failResult);
		}
	}

	class TestCounts
	{
		public int Total;
		public int Passed;
		public int Failed;
		public int Skipped;
	}
}
