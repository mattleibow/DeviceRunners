using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute for test methods that should be run on the UI thread using xUnit v3.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(DeviceRunners.UITesting.Xunit3.UIFactDiscoverer))]
public class UIFactAttribute : FactAttribute
{
}
