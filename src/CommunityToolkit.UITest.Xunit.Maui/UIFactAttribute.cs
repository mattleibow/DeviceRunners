using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Extensions.UITest.UIFactDiscoverer", "CommunityToolkit.UITest.Xunit.Maui")]
public class UIFactAttribute : FactAttribute
{
}
