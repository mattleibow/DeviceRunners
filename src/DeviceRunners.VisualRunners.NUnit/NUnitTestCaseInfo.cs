using NUnit.Framework.Interfaces;

namespace DeviceRunners.VisualRunners.NUnit;

class NUnitTestCaseInfo : ITestCaseInfo
{
	public NUnitTestCaseInfo(NUnitTestAssemblyInfo assembly, ITest testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));

		TestClassName = testCase.ClassName;
		TestMethodName = testCase.MethodName;
		Traits = ConvertTraits(testCase.Properties);
	}

	public NUnitTestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public string DisplayName => TestCase.FullName;

	public string? TestClassName { get; }

	public string? TestMethodName { get; }

	public string? TestClassNamespace => GetNamespace(TestClassName);

	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	public ITest TestCase { get; }

	public NUnitTestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(NUnitTestResultInfo result)
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

	static IReadOnlyDictionary<string, IReadOnlyList<string>> ConvertTraits(IPropertyBag? properties)
	{
		if (properties is null || properties.Keys.Count == 0)
			return new Dictionary<string, IReadOnlyList<string>>();

		var result = new Dictionary<string, IReadOnlyList<string>>(properties.Keys.Count);
		foreach (var key in properties.Keys)
		{
			var values = new List<string>();
			foreach (var value in properties[key])
			{
				if (value is not null)
					values.Add(value.ToString()!);
			}

			result[key] = values;
		}

		return result;
	}
}