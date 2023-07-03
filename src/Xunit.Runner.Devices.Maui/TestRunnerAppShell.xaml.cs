using Xunit.Runner.Devices.Maui.Pages;

namespace Xunit.Runner.Devices.Maui;

public partial class TestRunnerAppShell : Shell
{
	public TestRunnerAppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("testrunner/assembly", typeof(TestAssemblyPage));
		Routing.RegisterRoute("testrunner/assembly/result", typeof(TestResultPage));
	}
}
