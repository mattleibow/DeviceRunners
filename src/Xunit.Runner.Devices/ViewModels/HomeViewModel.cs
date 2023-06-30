using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Xunit.Runner.Devices;

public class HomeViewModel : AbstractBaseViewModel
{
	readonly ITestRunner _runner;

	string _diagnosticMessages = string.Empty;
	bool _loaded;
	bool _isBusy;

	public HomeViewModel(ITestRunner runner)
	{
		_runner = runner;

		_runner.OnDiagnosticMessage += RunnerOnOnDiagnosticMessage;

		TestAssemblies = new ObservableCollection<TestAssemblyViewModel>();

		RunEverythingCommand = new Command(RunEverythingExecute, () => !_isBusy);
		NavigateToTestAssemblyCommand = new Command<TestAssemblyViewModel?>(NavigateToTestAssemblyExecute);
	}

	public ObservableCollection<TestAssemblyViewModel> TestAssemblies { get; private set; }

	public ICommand RunEverythingCommand { get; }

	public ICommand NavigateToTestAssemblyCommand { get; }

	public event EventHandler<TestAssemblyViewModel>? TestAssemblySelected;

	public bool IsBusy
	{
		get => _isBusy;
		private set => Set(ref _isBusy, value, ((Command)RunEverythingCommand).ChangeCanExecute);
	}

	public string DiagnosticMessages
	{
		get => _diagnosticMessages;
		private set => Set(ref _diagnosticMessages, value);
	}

	public async Task StartAssemblyScanAsync()
	{
		if (_loaded)
			return;

		IsBusy = true;

		try
		{
			var allTests = await _runner.DiscoverAsync();

			TestAssemblies = new ObservableCollection<TestAssemblyViewModel>(allTests);
			RaisePropertyChanged(nameof(TestAssemblies));
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

			if (!string.IsNullOrWhiteSpace(DiagnosticMessages))
				DiagnosticMessages += $"----------{Environment.NewLine}";

			await _runner.RunAsync(TestAssemblies.Select(t => t.RunInfo).ToList(), "Run Everything");
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

	void RunnerOnOnDiagnosticMessage(string s)
	{
		DiagnosticMessages += $"{s}{Environment.NewLine}{Environment.NewLine}";
	}
}
