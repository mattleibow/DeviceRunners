namespace DeviceRunners.VisualRunners.MSTest;

class MSTestTestCaseInfo : ITestCaseInfo
{
	public MSTestTestCaseInfo(
		MSTestTestAssemblyInfo assembly,
		string uid,
		string displayName,
		string? testClassNamespace,
		string? testClassName,
		string? testMethodName,
		IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		Uid = uid ?? throw new ArgumentNullException(nameof(uid));
		DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
		TestClassNamespace = testClassNamespace;
		TestClassName = testClassName;
		TestMethodName = testMethodName;
		Traits = traits ?? new Dictionary<string, IReadOnlyList<string>>();
	}

	/// <summary>
	/// Builds a test case from a discovered server-mode node, deriving the class namespace and
	/// bare method name from the platform's <c>location.type</c> / <c>location.method</c> fields.
	/// </summary>
	public static MSTestTestCaseInfo FromDiscoveredNode(MSTestTestAssemblyInfo assembly, WireTestNode node)
	{
		var className = string.IsNullOrEmpty(node.LocationType) ? null : node.LocationType;
		var classNamespace = GetNamespace(className);
		var methodName = StripParameters(node.LocationMethod);

		return new MSTestTestCaseInfo(assembly, node.Uid, node.DisplayName, classNamespace, className, methodName, node.Traits);
	}

	static string? GetNamespace(string? className)
	{
		if (string.IsNullOrEmpty(className))
			return null;

		var index = className!.LastIndexOf('.');
		return index > 0 ? className.Substring(0, index) : null;
	}

	static string? StripParameters(string? method)
	{
		if (string.IsNullOrEmpty(method))
			return method;

		var index = method!.IndexOf('(');
		return index > 0 ? method.Substring(0, index) : method;
	}

	public MSTestTestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	/// <summary>
	/// The Microsoft.Testing.Platform test node UID. Used to filter execution to this specific
	/// test via the <c>--filter-uid</c> option, and to match result messages back to this case.
	/// </summary>
	public string Uid { get; }

	public string DisplayName { get; }

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
}
