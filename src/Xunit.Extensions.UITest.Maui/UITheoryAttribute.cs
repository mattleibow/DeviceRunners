using Xunit.Sdk;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("Xunit.Extensions.UITest.UITheoryDiscoverer", "Xunit.Extensions.UITest")]
public class UITheoryAttribute : TheoryAttribute
{
}
