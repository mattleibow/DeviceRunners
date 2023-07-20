using Xunit.Sdk;

namespace DeviceRunners.UITesting.Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("DeviceRunners.UITesting.Xunit.UIFactDiscoverer", "UITest.Xunit.Maui")]
public class UIFactAttribute : FactAttribute
{
}
