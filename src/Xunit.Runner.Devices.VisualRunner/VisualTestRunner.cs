using System.Diagnostics;
using System.Reflection;

using Microsoft.Extensions.Logging;

namespace Xunit.Runner.Devices.VisualRunner;

public class VisualTestRunner : ITestListener, ITestRunner
{
	readonly SynchronizationContext context = SynchronizationContext.Current;
	readonly AsyncLock executionLock = new();
	readonly IDiagnosticsManager _diagnosticsManager;
	readonly TestRunLogger _logger;
	volatile bool cancelled;

	public VisualTestRunner(RunnerOptions options, IDiagnosticsManager diagnosticsManager, ILogger<VisualTestRunner> logger)
	{
		_logger = new TestRunLogger(logger);
		_diagnosticsManager = diagnosticsManager;

		TestAssemblies = options.Assemblies;
	}

	public IReadOnlyCollection<Assembly> TestAssemblies { get; }

	public void RecordResult(TestResultViewModel result)
	{
		_logger.LogTestResult(result);
	}

	public Task RunAsync(TestCaseViewModel test)
	{
		return RunAsync(new[] { test });
	}

	public Task RunAsync(IEnumerable<TestCaseViewModel> tests, string? message = null)
	{
		var groups = tests
			.GroupBy(t => t.AssemblyFileName)
			.Select(g => new AssemblyRunInfo(
				g.Key,
				GetConfiguration(Path.GetFileNameWithoutExtension(g.Key)),
				g.ToList()))
			.ToList();

		return RunAsync(groups, message);
	}

	public async Task RunAsync(IReadOnlyList<AssemblyRunInfo> runInfos, string? message = null)
	{
		using (await executionLock.LockAsync())
		{
			message ??= runInfos.Count > 1 || runInfos.FirstOrDefault()?.TestCases.Count > 1
				? "Run Multiple Tests"
				: runInfos.FirstOrDefault()?.TestCases.FirstOrDefault()?.DisplayName;

			_logger.LogTestStart(message);

			try
			{
				await RunTests(() => runInfos);
			}
			finally
			{
				_logger.LogTestComplete();
			}
		}
	}

	public Task<IReadOnlyList<TestAssemblyViewModel>> DiscoverAsync()
	{
		var tcs = new TaskCompletionSource<IReadOnlyList<TestAssemblyViewModel>>();

		RunAsync(() =>
		{
			try
			{
				var runInfos = DiscoverTestsInAssemblies();
				var list = runInfos.Select(ri => new TestAssemblyViewModel(ri, this)).ToList();

				tcs.SetResult(list);
			}
			catch (Exception e)
			{
				tcs.SetException(e);
			}
		});

		return tcs.Task;
	}

	IEnumerable<AssemblyRunInfo> DiscoverTestsInAssemblies()
	{
		var result = new List<AssemblyRunInfo>();

		try
		{
			foreach (var assm in TestAssemblies)
			{
				var assemblyFileName = FileSystemUtils.GetAssemblyFileName(assm);
				var configuration = GetConfiguration(Path.GetFileNameWithoutExtension(assemblyFileName));
				var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);

				try
				{
					if (cancelled)
						break;

					using var framework = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName, null, false);
					using var sink = new TestDiscoverySink(() => cancelled);
					framework.Find(false, sink, discoveryOptions);
					sink.Finished.WaitOne();

					result.Add(new AssemblyRunInfo(
						assemblyFileName,
						configuration,
						sink.TestCases.Select(tc => new TestCaseViewModel(assemblyFileName, tc)).ToList()));
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}
			}
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
		}

		return result;
	}

	static TestAssemblyConfiguration GetConfiguration(string assemblyName)
	{
		using var stream = GetConfigurationStreamForAssembly(assemblyName);
		if (stream is not null)
			return ConfigReader.Load(stream);

		return new TestAssemblyConfiguration();
	}

	static Stream? GetConfigurationStreamForAssembly(string assemblyName)
	{
		var stream = FileSystemUtils.OpenAppPackageFile($"{assemblyName}.xunit.runner.json");
		if (stream is not null)
			return stream;

		stream = FileSystemUtils.OpenAppPackageFile($"xunit.runner.json");
		if (stream is not null)
			return stream;

		return null;
	}

	Task RunTests(Func<IReadOnlyList<AssemblyRunInfo>> testCaseAccessor)
	{
		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		void Handler()
		{
			var toDispose = new List<IDisposable>();

			try
			{
				cancelled = false;
				var assemblies = testCaseAccessor();
				var parallelizeAssemblies = assemblies.All(runInfo => runInfo.Configuration.ParallelizeAssemblyOrDefault);

				if (parallelizeAssemblies)
				{
					assemblies
						.Select(runInfo => RunTestsInAssemblyAsync(toDispose, runInfo))
						.ToList()
						.ForEach(@event => @event.WaitOne());
				}
				else
				{
					foreach (var runInfo in assemblies)
					{
						RunTestsInAssembly(toDispose, runInfo);
					}
				}
			}
			catch (Exception e)
			{
				tcs.SetException(e);
			}
			finally
			{
				toDispose.ForEach(disposable => disposable.Dispose());
				tcs.TrySetResult();
			}
		}

		RunAsync(Handler);

		return tcs.Task;
	}

	void RunTestsInAssembly(List<IDisposable> toDispose, AssemblyRunInfo runInfo)
	{
		if (cancelled)
			return;

		var assemblyFileName = runInfo.AssemblyFileName;

		var longRunningSeconds = runInfo.Configuration.LongRunningTestSecondsOrDefault;

		var controller = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName);

		lock (toDispose)
			toDispose.Add(controller);

		var xunitTestCases = runInfo.TestCases
			.Select(tc => new { vm = tc, tc = tc.TestCase })
			.Where(tc => tc.tc.UniqueID != null)
			.ToDictionary(tc => tc.tc, tc => tc.vm);

		var executionOptions = TestFrameworkOptions.ForExecution(runInfo.Configuration);

		var diagSink = new DiagnosticMessageSink(d => context.Post(_ => _diagnosticsManager.PostDiagnosticMessage(d), null), runInfo.AssemblyFileName, executionOptions.GetDiagnosticMessagesOrDefault());

		var deviceExecSink = new DeviceExecutionSink(xunitTestCases, this, context);

		IExecutionSink resultsSink = new DelegatingExecutionSummarySink(deviceExecSink, () => cancelled);
		if (longRunningSeconds > 0)
			resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagSink);

		var assm = new XunitProjectAssembly() { AssemblyFilename = runInfo.AssemblyFileName };
		deviceExecSink.OnMessage(new TestAssemblyExecutionStarting(assm, executionOptions));

		controller.RunTests(xunitTestCases.Select(tc => tc.Value.TestCase).ToList(), resultsSink, executionOptions);
		resultsSink.Finished.WaitOne();

		deviceExecSink.OnMessage(new TestAssemblyExecutionFinished(assm, executionOptions, resultsSink.ExecutionSummary));
	}

	ManualResetEvent RunTestsInAssemblyAsync(List<IDisposable> toDispose, AssemblyRunInfo runInfo)
	{
		var @event = new ManualResetEvent(false);

		void Handler()
		{
			try
			{
				RunTestsInAssembly(toDispose, runInfo);
			}
			finally
			{
				@event.Set();
			}
		}

		RunAsync(Handler);

		return @event;
	}

	static async void RunAsync(Action action)
	{
		var task = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

		try
		{
			await task;
		}
		catch (Exception e)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
				Debug.WriteLine(e);
			}
		}
	}
}
