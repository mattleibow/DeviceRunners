namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3TestAssemblyInfo : ITestAssemblyInfo
{
	readonly Xunit3TestAssemblyConfiguration _configuration = new();

	public Xunit3TestAssemblyInfo(string assemblyFileName)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
	}

	public Xunit3TestAssemblyInfo(string assemblyFileName, IReadOnlyList<Xunit3TestCaseInfo> testCases)
		: this(assemblyFileName)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public Xunit3TestAssemblyInfo(Xunit3TestAssemblyInfo testAssembly, IReadOnlyList<Xunit3TestCaseInfo> testCases)
		: this(testAssembly.AssemblyFileName, testCases)
	{
	}

	public string AssemblyFileName { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<Xunit3TestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
