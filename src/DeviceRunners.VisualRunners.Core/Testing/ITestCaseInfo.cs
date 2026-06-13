namespace DeviceRunners.VisualRunners;

public interface ITestCaseInfo
{
ITestAssemblyInfo TestAssembly { get; }

string DisplayName { get; }

ITestResultInfo? Result { get; }

event Action<ITestResultInfo>? ResultReported;

/// <summary>
/// The fully qualified test class name (e.g. "MyNamespace.MyClass"), when available.
/// </summary>
string? TestClassName => null;

/// <summary>
/// The test method name (e.g. "MyMethod"), when available.
/// </summary>
string? TestMethodName => null;

/// <summary>
/// The namespace of the test class (e.g. "MyNamespace"). Derived from
/// <see cref="TestClassName"/> unless an implementation provides a better value.
/// </summary>
string? TestClassNamespace
{
get
{
var className = TestClassName;
if (string.IsNullOrEmpty(className))
return null;

var index = className!.LastIndexOf('.');
return index > 0 ? className.Substring(0, index) : null;
}
}

/// <summary>
/// The traits (categories) associated with the test case, keyed by trait name.
/// </summary>
IReadOnlyDictionary<string, IReadOnlyList<string>> Traits => EmptyTraits;

private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EmptyTraits =
new Dictionary<string, IReadOnlyList<string>>();
}
