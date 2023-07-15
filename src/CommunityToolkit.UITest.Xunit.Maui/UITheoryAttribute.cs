using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Extensions.UITest.UITheoryDiscoverer", "CommunityToolkit.UITest.Xunit.Maui")]
public class UITheoryAttribute : TheoryAttribute
{
}
