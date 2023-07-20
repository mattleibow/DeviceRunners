using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("DeviceRunners.UITesting.Xunit.UITheoryDiscoverer", "DeviceRunners.UITesting.Xunit")]
public class UITheoryAttribute : TheoryAttribute
{
}
