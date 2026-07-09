using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace DeviceRunners.VisualRunners.MSTest;

class MSTestTestCaseInfo : ITestCaseInfo
{
	// Property identifiers set by the MSTest adapter on each discovered TestCase.
	const string ManagedTypePropertyId = "TestCase.ManagedType";
	const string ManagedMethodPropertyId = "TestCase.ManagedMethod";
	const string TestCategoryPropertyId = "MSTestDiscoverer.TestCategory";

	// The trait key under which categories are exposed, matching the `dotnet test --filter`
	// "Category" property used by TestCaseFilter (and the NUnit backend).
	const string CategoryTraitName = "Category";

	public MSTestTestCaseInfo(MSTestTestAssemblyInfo assembly, TestCase testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));

		TestClassName = GetPropertyValue(testCase, ManagedTypePropertyId) ?? GetClassNameFromFullyQualifiedName(testCase.FullyQualifiedName);
		TestMethodName = StripParameters(GetPropertyValue(testCase, ManagedMethodPropertyId)) ?? GetMethodNameFromFullyQualifiedName(testCase.FullyQualifiedName);
		TestClassNamespace = GetNamespace(TestClassName);
		Traits = ConvertTraits(testCase);
	}

	public MSTestTestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public TestCase TestCase { get; }

	public Guid Id => TestCase.Id;

	public string DisplayName => TestCase.DisplayName ?? TestCase.FullyQualifiedName;

	public string? TestClassName { get; }

	public string? TestMethodName { get; }

	public string? TestClassNamespace { get; }

	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	public MSTestTestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(MSTestTestResultInfo result)
	{
		Result = result;

		ResultReported?.Invoke(result);
	}

	static string? GetPropertyValue(TestCase testCase, string propertyId)
	{
		foreach (var property in testCase.Properties)
		{
			if (string.Equals(property.Id, propertyId, StringComparison.Ordinal))
			{
				var value = testCase.GetPropertyValue(property);
				return value?.ToString();
			}
		}

		return null;
	}

	static string? StripParameters(string? methodName)
	{
		if (string.IsNullOrEmpty(methodName))
			return methodName;

		var index = methodName!.IndexOf('(');
		return index > 0 ? methodName.Substring(0, index) : methodName;
	}

	static string? GetClassNameFromFullyQualifiedName(string? fullyQualifiedName)
	{
		if (string.IsNullOrEmpty(fullyQualifiedName))
			return null;

		var index = fullyQualifiedName!.LastIndexOf('.');
		return index > 0 ? fullyQualifiedName.Substring(0, index) : null;
	}

	static string? GetMethodNameFromFullyQualifiedName(string? fullyQualifiedName)
	{
		if (string.IsNullOrEmpty(fullyQualifiedName))
			return null;

		var index = fullyQualifiedName!.LastIndexOf('.');
		return index >= 0 && index < fullyQualifiedName.Length - 1 ? fullyQualifiedName.Substring(index + 1) : fullyQualifiedName;
	}

	static string? GetNamespace(string? className)
	{
		if (string.IsNullOrEmpty(className))
			return null;

		var index = className!.LastIndexOf('.');
		return index > 0 ? className.Substring(0, index) : null;
	}

	static IReadOnlyDictionary<string, IReadOnlyList<string>> ConvertTraits(TestCase testCase)
	{
		var result = new Dictionary<string, IReadOnlyList<string>>();

		foreach (var trait in testCase.Traits)
		{
			if (trait.Name is null)
				continue;

			AddTraitValue(result, trait.Name, trait.Value);
		}

		// MSTest surfaces [TestCategory] via a dedicated property rather than as a trait; expose
		// each category under the "Category" trait so `dotnet test --filter Category=...` works.
		foreach (var property in testCase.Properties)
		{
			if (!string.Equals(property.Id, TestCategoryPropertyId, StringComparison.Ordinal))
				continue;

			if (testCase.GetPropertyValue(property) is string[] categories)
				foreach (var category in categories)
					AddTraitValue(result, CategoryTraitName, category);
		}

		return result;
	}

	static void AddTraitValue(Dictionary<string, IReadOnlyList<string>> traits, string name, string? value)
	{
		if (value is null)
			return;

		if (!traits.TryGetValue(name, out var values))
		{
			values = new List<string>();
			traits[name] = values;
		}

		((List<string>)values).Add(value);
	}
}
