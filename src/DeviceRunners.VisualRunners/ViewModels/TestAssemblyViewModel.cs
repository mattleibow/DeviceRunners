using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace DeviceRunners.VisualRunners;

public class TestAssemblyViewModel : AbstractBaseViewModel
{
	readonly ObservableCollection<TestCaseViewModel> _allTests;
	readonly FilteredCollectionView<TestCaseViewModel, FilterArgs> _filteredTests;
	readonly ITestRunner _runner;
	readonly List<TestCaseViewModel> _results;

	CancellationTokenSource? _filterCancellationTokenSource;
	TestState _result;
	TestState _resultFilter;
	TestResultStatus _runStatus;
	string? _searchQuery;

	string? _detailText;
	string? _displayName;

	bool _isBusy;
	int _notRun;
	int _passed;
	int _failed;
	int _skipped;

	public TestAssemblyViewModel(ITestAssemblyInfo testAssembly, ITestRunner testRunner)
	{
		_runner = testRunner;

		TestAssemblyInfo = testAssembly;

		RunAllTestsCommand = new Command(RunAllTestsExecute, () => !_isBusy);
		RunFilteredTestsCommand = new Command(RunFilteredTestsExecute, () => !_isBusy);
		NavigateToResultCommand = new Command<TestCaseViewModel?>(NavigateToResultExecute, tc => !_isBusy);

		DisplayName = Path.GetFileNameWithoutExtension(testAssembly.AssemblyFileName);

		var testCases = testAssembly.TestCases
			.Select(t => new TestCaseViewModel(t))
			.ToList();
		_allTests = new ObservableCollection<TestCaseViewModel>(testCases);
		_results = new List<TestCaseViewModel>(testCases);

		_allTests.CollectionChanged += (_, args) =>
		{
			lock (_results)
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						foreach (TestCaseViewModel item in args.NewItems!)
							_results.Add(item);
						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (TestCaseViewModel item in args.OldItems!)
							_results.Remove(item);
						break;
					default:
						throw new InvalidOperationException($"I can't work with {args.Action}");
				}
			}
		};

		_filteredTests = new FilteredCollectionView<TestCaseViewModel, FilterArgs>(
			_allTests,
			IsTestFilterMatch,
			new FilterArgs(SearchQuery, ResultFilter),
			new TestComparer());

		_filteredTests.ItemChanged += (sender, args) => UpdateCaption();
		_filteredTests.CollectionChanged += (sender, args) => UpdateCaption();

		Result = TestState.NotRun;
		ResultStatus = TestResultStatus.NotRun;

		UpdateCaption();
	}

	public ITestAssemblyInfo TestAssemblyInfo { get; }

	public ICommand RunAllTestsCommand { get; }

	public ICommand RunFilteredTestsCommand { get; }

	public ICommand NavigateToResultCommand { get; }

	public event EventHandler<TestResultViewModel>? TestResultSelected;

	public IList<TestCaseViewModel> TestCases => _filteredTests;

	public string DetailText
	{
		get => _detailText ?? string.Empty;
		private set => Set(ref _detailText, value);
	}

	public string DisplayName
	{
		get => _displayName ?? string.Empty;
		private set => Set(ref _displayName, value);
	}

	public bool IsBusy
	{
		get => _isBusy;
		private set
		{
			if (Set(ref _isBusy, value))
			{
				((Command)RunAllTestsCommand).ChangeCanExecute();
				((Command)RunFilteredTestsCommand).ChangeCanExecute();
			}
		}
	}

	public TestState Result
	{
		get => _result;
		set => Set(ref _result, value);
	}

	public TestState ResultFilter
	{
		get => _resultFilter;
		set => Set(ref _resultFilter, value, FilterAfterDelay);
	}

	public TestResultStatus ResultStatus
	{
		get => _runStatus;
		private set => Set(ref _runStatus, value);
	}

	public string SearchQuery
	{
		get => _searchQuery ?? string.Empty;
		set => Set(ref _searchQuery, value, FilterAfterDelay);
	}

	public int NotRun
	{
		get => _notRun;
		set => Set(ref _notRun, value);
	}

	public int Passed
	{
		get => _passed;
		set => Set(ref _passed, value);
	}

	public int Failed
	{
		get => _failed;
		set => Set(ref _failed, value);
	}

	public int Skipped
	{
		get => _skipped;
		set => Set(ref _skipped, value);
	}

	void FilterAfterDelay()
	{
		_filterCancellationTokenSource?.Cancel();
		_filterCancellationTokenSource = new CancellationTokenSource();

		var token = _filterCancellationTokenSource.Token;

		Task.Delay(500, token)
			.ContinueWith(
				x => { _filteredTests.FilterArgument = new FilterArgs(SearchQuery, ResultFilter); },
				token,
				TaskContinuationOptions.None,
				TaskScheduler.FromCurrentSynchronizationContext());
	}

	static bool IsTestFilterMatch(TestCaseViewModel test, FilterArgs query)
	{
		if (test == null)
			throw new ArgumentNullException(nameof(test));

		var state = query.State;
		var pattern = query.Query.Trim();

		TestState? requiredTestState = state switch
		{
			TestState.All => null,
			TestState.Passed => TestState.Passed,
			TestState.Failed => TestState.Failed,
			TestState.Skipped => TestState.Skipped,
			TestState.NotRun => TestState.NotRun,
			_ => throw new ArgumentException(nameof(state)),
		};

		if (requiredTestState.HasValue && GetTestState(test.ResultStatus) != requiredTestState.Value)
			return false;

		return
			string.IsNullOrEmpty(pattern) ||
			test.DisplayName.Contains(pattern, StringComparison.OrdinalIgnoreCase);
	}

	async void RunAllTestsExecute()
	{
		try
		{
			IsBusy = true;
			await _runner.RunTestsAsync(TestAssemblyInfo);
		}
		finally
		{
			IsBusy = false;
		}
	}

	async void RunFilteredTestsExecute()
	{
		try
		{
			IsBusy = true;
			await _runner.RunTestsAsync(_filteredTests.Select(vm => vm.TestCaseInfo));
		}
		finally
		{
			IsBusy = false;
		}
	}

	async void NavigateToResultExecute(TestCaseViewModel? testCase)
	{
		if (testCase == null)
			return;

		await _runner.RunTestsAsync(testCase.TestCaseInfo);

		TestResultSelected?.Invoke(this, testCase.TestResult);
	}

	void UpdateCaption()
	{
		var count = _allTests.Count;

		if (count == 0)
		{
			DetailText = "no test was found inside this assembly";
			ResultStatus = TestResultStatus.NoTests;

			return;
		}

		// This would occasionally crash when running the group operation
		// most likely because of thread safety issues.
		Dictionary<TestState, int> results;
		lock (_results)
		{
			results =
				_results
					.GroupBy(r => GetTestState(r.ResultStatus))
					.ToDictionary(k => k.Key, v => v.Count());
		}

		results.TryGetValue(TestState.Passed, out int passed);
		results.TryGetValue(TestState.Failed, out int failure);
		results.TryGetValue(TestState.Skipped, out int skipped);
		results.TryGetValue(TestState.NotRun, out int notRun);

		Passed = passed;
		Failed = failure;
		Skipped = skipped;
		NotRun = notRun;

		var prefix = notRun == 0 ? "Complete - " : string.Empty;

		if (failure == 0 && notRun == 0)
		{
			// No failures and all run

			DetailText = $"{prefix}✔ {passed}";
			ResultStatus = TestResultStatus.Passed;

			Result = TestState.Passed;
		}
		else if (failure > 0 || (notRun > 0 && notRun < count))
		{
			// Either some failed or some are not run

			DetailText = $"{prefix}✔ {passed}, ⛔ {failure}, ⚠ {skipped}, 🔷 {notRun}";

			if (failure > 0) // always show a fail
			{
				ResultStatus = TestResultStatus.Failed;
				Result = TestState.Failed;
			}
			else
			{
				if (passed > 0)
				{
					ResultStatus = TestResultStatus.Passed;
					Result = TestState.Passed;
				}
				else if (skipped > 0)
				{
					ResultStatus = TestResultStatus.Skipped;
					Result = TestState.Skipped;
				}
				else
				{
					// just not run
					ResultStatus = TestResultStatus.NotRun;
					Result = TestState.NotRun;
				}
			}
		}
		else if (Result == TestState.NotRun)
		{
			// Not run

			DetailText = $"🔷 {count}, {Result}";
			ResultStatus = TestResultStatus.NotRun;
		}
	}

	static TestState GetTestState(TestResultStatus status) =>
		status switch
		{
			TestResultStatus.Passed => TestState.Passed,
			TestResultStatus.Failed => TestState.Failed,
			TestResultStatus.Skipped => TestState.Skipped,
			_ => TestState.NotRun,
		};

	class TestComparer : IComparer<TestCaseViewModel>
	{
		public int Compare(TestCaseViewModel? x, TestCaseViewModel? y) =>
			string.Compare(x?.DisplayName, y?.DisplayName, StringComparison.OrdinalIgnoreCase);
	}

	record FilterArgs(string Query, TestState State);
}
