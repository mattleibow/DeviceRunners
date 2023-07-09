using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CommunityToolkit.DeviceRunners.Xunit.VisualRunner;

public class HomeViewModel : AbstractBaseViewModel
{
	readonly ITestRunner _runner;
	readonly RunnerOptions _runnerOptions;
	readonly IDiagnosticsManager _diagnosticsManager;

	bool _isLoaded;
	bool _isBusy;

	public HomeViewModel(ITestRunner runner, RunnerOptions options, IDiagnosticsManager diagnosticsManager, DiagnosticsViewModel diagnosticsViewModel)
	{
		_runner = runner;
		_runnerOptions = options;
		_diagnosticsManager = diagnosticsManager;

		Diagnostics = diagnosticsViewModel;

		RunEverythingCommand = new Command(RunEverythingExecute, () => !_isBusy);
		NavigateToTestAssemblyCommand = new Command<TestAssemblyViewModel?>(NavigateToTestAssemblyExecute);
	}

	public DiagnosticsViewModel Diagnostics { get; }

	public ObservableCollection<TestAssemblyViewModel> TestAssemblies { get; } = new();

	public ICommand RunEverythingCommand { get; }

	public ICommand NavigateToTestAssemblyCommand { get; }

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
		private set => Set(ref _isLoaded, value, () => AssemblyScanCompleted?.Invoke(this, EventArgs.Empty));
	}

	public async Task StartAssemblyScanAsync()
	{
		if (IsLoaded)
			return;
		IsBusy = true;

		_diagnosticsManager.PostDiagnosticMessage("Discovering test assemblies...");

		try
		{
			var allTests = await _runner.DiscoverAsync();

			foreach (var vm in allTests)
			{
				TestAssemblies.Add(vm);
			}

			_diagnosticsManager.PostDiagnosticMessage($"Discovered {allTests.Count} test assemblies.");
		}
		catch (Exception ex)
		{
			_diagnosticsManager.PostDiagnosticMessage($"Error during test discovery: '{ex.Message}'{Environment.NewLine}{ex}");
		}

		IsLoaded = true;
		IsBusy = false;
	}

	async Task RunEverythingAsync()
	{
		_diagnosticsManager.PostDiagnosticMessage("Starting a new test run of everything...");

		try
		{
			await _runner.RunAsync(TestAssemblies.Select(t => t.RunInfo).ToList(), "Run Everything");
		}
		finally
		{
			_diagnosticsManager.PostDiagnosticMessage("Test run complete.");
		}
	}

	async void RunEverythingExecute()
	{
		IsBusy = true;

		try
		{
			await RunEverythingAsync();
		}
		finally
		{
			IsBusy = false;
		}
	}

	void NavigateToTestAssemblyExecute(TestAssemblyViewModel? vm)
	{
		if (vm is null)
			return;

		TestAssemblySelected?.Invoke(this, vm);
	}
}
