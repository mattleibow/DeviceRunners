using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitTestCaseInfo : ITestCaseInfo
{
	public XunitTestCaseInfo(XunitTestAssemblyInfo assembly, ITestCase testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));

		TestClassName = testCase.TestMethod?.TestClass?.Class?.Name;
		TestMethodName = testCase.TestMethod?.Method?.Name;
		TestClassNamespace = GetNamespace(TestClassName);
		Traits = ConvertTraits(testCase.Traits);
	}

	public XunitTestAssemblyInfo TestAssembly { get; }
	
	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public string DisplayName => TestCase.DisplayName;

	public string? TestClassName { get; }

	public string? TestMethodName { get; }

	public string? TestClassNamespace { get; }

	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	public ITestCase TestCase { get; }

	public XunitTestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(XunitTestResultInfo result)
	{
		Result = result;

		ResultReported?.Invoke(result);
	}

	static string? GetNamespace(string? className)
	{
		if (string.IsNullOrEmpty(className))
			return null;

		var index = className!.LastIndexOf('.');
		return index > 0 ? className.Substring(0, index) : null;
	}

	static IReadOnlyDictionary<string, IReadOnlyList<string>> ConvertTraits(Dictionary<string, List<string>>? traits)
	{
		if (traits is null || traits.Count == 0)
			return new Dictionary<string, IReadOnlyList<string>>();

		var result = new Dictionary<string, IReadOnlyList<string>>(traits.Count);
		foreach (var pair in traits)
			result[pair.Key] = pair.Value;

		return result;
	}
}
