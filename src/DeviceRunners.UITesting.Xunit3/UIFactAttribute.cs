using DeviceRunners.UITesting.Xunit3;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute for test methods that should be run on the UI thread using xUnit v3.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(typeof(UIFactDiscoverer))]
public class UIFactAttribute : FactAttribute
{
}
