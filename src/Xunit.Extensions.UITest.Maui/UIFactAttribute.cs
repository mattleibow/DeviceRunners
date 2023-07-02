using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Extensions.UITest.UIFactDiscoverer", "Xunit.Extensions.UITest")]
public class UIFactAttribute : FactAttribute
{
}
