namespace DeviceRunners.VisualRunners.MSTest;

class MSTestTestAssemblyInfo : ITestAssemblyInfo
{
	readonly ITestAssemblyConfiguration _configuration = new MSTestTestAssemblyConfiguration();

	public MSTestTestAssemblyInfo(string assemblyFileName)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
	}

	public MSTestTestAssemblyInfo(string assemblyFileName, IReadOnlyList<MSTestTestCaseInfo> testCases)
		: this(assemblyFileName)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public MSTestTestAssemblyInfo(MSTestTestAssemblyInfo testAssembly, IReadOnlyList<MSTestTestCaseInfo> testCases)
		: this(testAssembly.AssemblyFileName, testCases)
	{
	}

	public string AssemblyFileName { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<MSTestTestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
