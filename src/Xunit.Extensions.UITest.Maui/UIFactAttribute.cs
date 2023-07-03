using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Extensions.UITest.UIFactDiscoverer", "Xunit.Extensions.UITest.Maui")]
public class UIFactAttribute : FactAttribute
{
}
