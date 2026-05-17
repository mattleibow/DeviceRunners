using Xunit;
using Xunit.v3;

namespace DeviceRunners.UITesting.Xunit3;

/// <summary>
/// Attribute for test methods that should be run on the UI thread using xUnit v3.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(UIFactDiscoverer))]
public class UIFactAttribute : FactAttribute
{
}
