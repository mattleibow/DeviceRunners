using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("DeviceRunners.UITesting.Xunit.UITheoryDiscoverer", "UITest.Xunit.Maui")]
public class UITheoryAttribute : TheoryAttribute
{
}
