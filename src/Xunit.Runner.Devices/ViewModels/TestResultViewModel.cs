using Xunit.Abstractions;

namespace Xunit.Runner.Devices;

public class TestResultViewModel : AbstractBaseViewModel
{
	TimeSpan _duration;
	string? _errorMessage;
	string? _errorStackTrace;
	byte[]? _errorImage;

	public TestResultViewModel(TestCaseViewModel testCase, ITestResultMessage? testResult)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		TestResultMessage = testResult;

		if (testResult is not null)
			testCase.UpdateTestState(this);
	}

	public TestCaseViewModel TestCase { get; }

	public ITestResultMessage? TestResultMessage { get; }

	public TimeSpan Duration
	{
		get => _duration;
		set => Set(ref _duration, value, () => TestCase?.UpdateTestState(this));
	}

	public string? ErrorMessage
	{
		get => _errorMessage;
		set
		{
			if (Set(ref _errorMessage, value))
				ExtractErrorMessage(value);
		}
	}

	public string? ErrorStackTrace
	{
		get => _errorStackTrace;
		set => Set(ref _errorStackTrace, value);
	}

	public byte[]? ErrorImage
	{
		get => _errorImage;
		set => Set(ref _errorImage, value);
	}

	public string? Output => TestResultMessage?.Output;

	public bool HasOutput => !string.IsNullOrWhiteSpace(Output);

	void ExtractErrorMessage(string? message)
	{
		if (message is null)
		{
			ErrorImage = null;
			return;
		}

		const string openTag = "<img>";
		const string closeTag = "</img>";

		var openTagIndex = message.IndexOf(openTag);
		var closeTagIndex = message.IndexOf(closeTag);

		if (openTagIndex >= 0 && closeTagIndex > openTagIndex)
		{
			var start = openTagIndex + openTag.Length;
			var end = closeTagIndex - openTagIndex - openTag.Length;
			var imgString = message.Substring(start, end);
			ErrorImage = Convert.FromBase64String(imgString);
		}
	}
}
