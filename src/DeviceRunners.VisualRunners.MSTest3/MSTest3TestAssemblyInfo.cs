namespace DeviceRunners.VisualRunners.MSTest3;

class MSTest3TestAssemblyInfo : ITestAssemblyInfo
{
	readonly ITestAssemblyConfiguration _configuration = new MSTest3TestAssemblyConfiguration();

	public MSTest3TestAssemblyInfo(string assemblyFileName)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
	}

	public MSTest3TestAssemblyInfo(string assemblyFileName, IReadOnlyList<MSTest3TestCaseInfo> testCases)
		: this(assemblyFileName)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public MSTest3TestAssemblyInfo(MSTest3TestAssemblyInfo testAssembly, IReadOnlyList<MSTest3TestCaseInfo> testCases)
		: this(testAssembly.AssemblyFileName, testCases)
	{
	}

	public string AssemblyFileName { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<MSTest3TestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
