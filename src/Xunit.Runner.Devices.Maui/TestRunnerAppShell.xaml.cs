using Xunit.Runner.Devices.Maui.Pages;

namespace Xunit.Runner.Devices.Maui;

public partial class TestRunnerAppShell : Shell
{
	public TestRunnerAppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("runner/assembly", typeof(TestAssemblyPage));
		Routing.RegisterRoute("runner/assembly/result", typeof(TestResultPage));
	}
}
