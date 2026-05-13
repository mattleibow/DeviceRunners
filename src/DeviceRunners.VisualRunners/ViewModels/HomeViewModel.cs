using System.Collections.ObjectModel;
using System.Windows.Input;

using DeviceRunners.Core;

namespace DeviceRunners.VisualRunners;

public class HomeViewModel : AbstractBaseViewModel
{
	readonly IVisualTestRunnerConfiguration _options;
	readonly ITestDiscoverer _discoverer;
	readonly ITestRunner _runner;
	readonly IAppTerminator? _appTerminator;

	readonly IDiagnosticsManager? _diagnosticsManager;

	bool _isLoaded;
	bool _isBusy;
	TestAssemblyViewModel? _selectedTestAssembly;

	public HomeViewModel(
		IVisualTestRunnerConfiguration options,
		IEnumerable<ITestDiscoverer> testDiscoverers,
		IEnumerable<ITestRunner> testRunners,
		IResultChannelManager resultChannelManager,
		IAppTerminator? appTerminator = null,
		IDiagnosticsManager? diagnosticsManager = null,
		DiagnosticsViewModel? diagnosticsViewModel = null)
	{
		_options = options;
		_discoverer = new CompositeTestDiscoverer(testDiscoverers);
		_runner = new CompositeTestRunner(options, resultChannelManager, testRunners);

		_appTerminator = appTerminator;
		_diagnosticsManager = diagnosticsManager;

		Diagnostics = diagnosticsViewModel;

		RunEverythingCommand = new Command(RunEverythingExecute, () => !_isBusy);
	}

	public DiagnosticsViewModel? Diagnostics { get; }

	public ObservableCollection<TestAssemblyViewModel> TestAssemblies { get; } = new();

	public ICommand RunEverythingCommand { get; }

	public TestAssemblyViewModel? SelectedTestAssembly
	{
		get => _selectedTestAssembly;
		set
		{
			if (Set(ref _selectedTestAssembly, value) && value is not null)
			{
				TestAssemblySelected?.Invoke(this, value);
				// Clear selection so re-entering the page doesn't re-navigate
				_selectedTestAssembly = null;
				RaisePropertyChanged(nameof(SelectedTestAssembly));
			}
		}
	}

	public event EventHandler<TestAssemblyViewModel>? TestAssemblySelected;

	public event EventHandler? AssemblyScanCompleted;

	public bool IsBusy
	{
		get => _isBusy;
		private set => Set(ref _isBusy, value, ((Command)RunEverythingCommand).ChangeCanExecute);
	}

	public bool IsLoaded
	{
		get => _isLoaded;
		private set => Set(ref _isLoaded, value);
	}

	public async Task StartAssemblyScanAsync()
	{
		if (IsLoaded)
			return;
		IsBusy = true;

		_diagnosticsManager?.PostDiagnosticMessage("Discovering test assemblies...");

		try
		{
			var allTests = await _discoverer.DiscoverAsync();

			foreach (var assembly in allTests)
			{
				TestAssemblies.Add(new TestAssemblyViewModel(assembly, _runner));
			}

			_diagnosticsManager?.PostDiagnosticMessage($"Discovered {allTests.Count} test assemblies.");
		}
		catch (Exception ex)
		{
			_diagnosticsManager?.PostDiagnosticMessage($"Error during test discovery: '{ex.Message}'{Environment.NewLine}{ex}");
		}

		IsLoaded = true;
		IsBusy = false;

		AssemblyScanCompleted?.Invoke(this, EventArgs.Empty);

		if (_options.AutoStart)
		{
			_diagnosticsManager?.PostDiagnosticMessage("Auto-starting test run...");

			await RunEverythingAsync();

			if (_options.AutoTerminate)
			{
				_diagnosticsManager?.PostDiagnosticMessage("Auto-terminating test runner...");

				_appTerminator?.Terminate();
			}
		}
	}

	async Task RunEverythingAsync()
	{
		IsBusy = true;
		_diagnosticsManager?.PostDiagnosticMessage("Starting a new test run of everything...");

		try
		{
			await _runner.RunTestsAsync(TestAssemblies.Select(t => t.TestAssemblyInfo).ToList());
		}
		finally
		{
			_diagnosticsManager?.PostDiagnosticMessage("Test run complete.");
			IsBusy = false;
		}
	}

	async void RunEverythingExecute()
	{
		await RunEverythingAsync();
	}
}
