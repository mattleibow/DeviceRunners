namespace DeviceRunners.VisualRunners;

public class TestResultViewModel : AbstractBaseViewModel
{
	bool _parsedErrorImage;
	byte[]? _errorImage;

	public TestResultViewModel(TestCaseViewModel testCase, ITestResultInfo? testResult)
	{
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));

		TestResultInfo = testResult;
	}

	public TestCaseViewModel TestCase { get; }

	public ITestResultInfo? TestResultInfo { get; }

	public TestResultStatus ResultStatus => TestResultInfo?.Status ?? TestResultStatus.NotRun;

	public TimeSpan Duration => TestResultInfo?.Duration ?? TimeSpan.Zero;

	public string? ErrorMessage => TestResultInfo?.ErrorMessage;

	public string? ErrorStackTrace => TestResultInfo?.ErrorStackTrace;

	public byte[]? ErrorImage => GetErrorImage();

	public string? SkipReason => TestResultInfo?.SkipReason;

	public string? Output => TestResultInfo?.Output;

	public bool HasOutput => !string.IsNullOrWhiteSpace(Output);

	byte[]? GetErrorImage()
	{
		if (_parsedErrorImage)
			return _errorImage;

		_parsedErrorImage = true;

		if (ErrorMessage is not null)
		{
			const string openTag = "<img>";
			const string closeTag = "</img>";

			var openTagIndex = ErrorMessage.IndexOf(openTag);
			var closeTagIndex = ErrorMessage.IndexOf(closeTag);

			if (openTagIndex >= 0 && closeTagIndex > openTagIndex)
			{
				var start = openTagIndex + openTag.Length;
				var end = closeTagIndex - openTagIndex - openTag.Length;
				var imgString = ErrorMessage.Substring(start, end);

				_errorImage = Convert.FromBase64String(imgString);
			}
		}

		return _errorImage;
	}
}
