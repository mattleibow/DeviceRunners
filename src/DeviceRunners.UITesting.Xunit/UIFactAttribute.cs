using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("DeviceRunners.UITesting.Xunit.UIFactDiscoverer", "DeviceRunners.UITesting.Xunit")]
public class UIFactAttribute : FactAttribute
{
}
