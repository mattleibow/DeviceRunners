using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Xunit.Runner.Devices;

public class HomeViewModel : AbstractBaseViewModel
{
	readonly ITestRunner _runner;
	readonly DiagnosticsViewModel _diagnosticsViewModel;

	bool _loaded;
	bool _isBusy;
	ObservableCollection<TestAssemblyViewModel> _testAssemblies = new();

	public HomeViewModel(ITestRunner runner, DiagnosticsViewModel diagnosticsViewModel)
	{
		_runner = runner;
		_diagnosticsViewModel = diagnosticsViewModel;

		RunEverythingCommand = new Command(RunEverythingExecute, () => !_isBusy);
		NavigateToTestAssemblyCommand = new Command<TestAssemblyViewModel?>(NavigateToTestAssemblyExecute);
	}

	public DiagnosticsViewModel Diagnostics => _diagnosticsViewModel;

	public ObservableCollection<TestAssemblyViewModel> TestAssemblies
	{
		get => _testAssemblies;
		private set => Set(ref _testAssemblies, value);
	}

	public ICommand RunEverythingCommand { get; }

	public ICommand NavigateToTestAssemblyCommand { get; }

	public event EventHandler<TestAssemblyViewModel>? TestAssemblySelected;

	public bool IsBusy
	{
		get => _isBusy;
		private set => Set(ref _isBusy, value, ((Command)RunEverythingCommand).ChangeCanExecute);
	}

	public async Task StartAssemblyScanAsync()
	{
		if (_loaded)
			return;

		IsBusy = true;

		_diagnosticsViewModel.PostDiagnosticMessage("Discovering test assemblies...");

		try
		{
			var allTests = await _runner.DiscoverAsync();

			TestAssemblies = new ObservableCollection<TestAssemblyViewModel>(allTests);
			RaisePropertyChanged(nameof(TestAssemblies));

			_diagnosticsViewModel.PostDiagnosticMessage($"Discovered {allTests.Count} test assemblies.");
		}
		finally
		{

			IsBusy = false;
			_loaded = true;
		}
	}

	async void RunEverythingExecute()
	{
		try
		{
			IsBusy = true;

			_diagnosticsViewModel.Clear();
			_diagnosticsViewModel.PostDiagnosticMessage("Starting a new test run of everything...");

			await _runner.RunAsync(TestAssemblies.Select(t => t.RunInfo).ToList(), "Run Everything");
		}
		finally
		{
			_diagnosticsViewModel.PostDiagnosticMessage("Test run complete.");

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
