namespace DeviceRunners.VisualRunners;

public class TestCaseViewModel : AbstractBaseViewModel
{
	TestResultStatus _resultStatus;
	string? _output;
	string? _message;
	string? _stackTrace;

	TestResultViewModel _testResult;

	// TestCases with strongly typed class data for some reason don't get split into different
	// test cases by the TheoryDiscoverer.
	// I've worked around it here for now so that a failed result won't get hidden when using the visual runner
	readonly Dictionary<string, ITestResultInfo> _testResults = new();

	public TestCaseViewModel(ITestCaseInfo testCase)
	{
		TestCaseInfo = testCase ?? throw new ArgumentNullException(nameof(testCase));

		// create an initial result representing not run
		_testResult = new TestResultViewModel(this, null);
		UpdateProperties();

		TestCaseInfo.ResultReported += OnTestResultReported;
	}

	public ITestCaseInfo TestCaseInfo { get; }

	public ITestAssemblyInfo TestAssemblyInfo => TestCaseInfo.TestAssembly;

	public string AssemblyFileName => TestAssemblyInfo.AssemblyFileName;

	public string DisplayName => TestCaseInfo.DisplayName;

	public TestResultStatus ResultStatus
	{
		get => _resultStatus;
		private set => Set(ref _resultStatus, value);
	}

	public string? Message
	{
		get => _message;
		private set => Set(ref _message, value);
	}

	public string? Output
	{
		get => _output;
		private set => Set(ref _output, value);
	}

	public string? StackTrace
	{
		get => _stackTrace;
		private set => Set(ref _stackTrace, value);
	}

	public TestResultViewModel TestResult
	{
		get => _testResult;
		private set => Set(ref _testResult, value, UpdateProperties);
	}

	void OnTestResultReported(ITestResultInfo testResult)
	{
		// add all the results to the collection for this test
		_testResults[testResult.TestCase.DisplayName] = testResult;

		// surface any failing tests up to the visual runner
		foreach (var result in _testResults.Values)
		{
			if (result.Status == TestResultStatus.Failed)
			{
				TestResult = new TestResultViewModel(this, result);
				return;
			}
		}

		// if none failed, then show the new result directly
		TestResult = new TestResultViewModel(this, testResult);
	}

	void UpdateProperties()
	{
		Output = TestResult.Output;

		if (TestResult.ResultStatus == TestResultStatus.Passed)
		{
			ResultStatus = TestResultStatus.Passed;
			Message = $"✔ Success! {TestResult.Duration.TotalMilliseconds} ms";
			StackTrace = null;
		}
		else if (TestResult.ResultStatus == TestResultStatus.Failed)
		{
			ResultStatus = TestResultStatus.Failed;
			Message = $"⛔ {TestResult.ErrorMessage}";
			StackTrace = TestResult.ErrorStackTrace;
		}
		else if (TestResult.ResultStatus == TestResultStatus.Skipped)
		{
			ResultStatus = TestResultStatus.Skipped;
			Message = $"⚠ {TestResult.SkipReason}";
		}
		else
		{
			ResultStatus = TestResultStatus.NotRun;
			Message = "🔷 not run";
			StackTrace = null;
		}
	}
}
