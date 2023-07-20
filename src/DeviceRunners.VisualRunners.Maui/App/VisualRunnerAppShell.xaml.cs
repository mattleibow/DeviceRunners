using DeviceRunners.VisualRunners.Maui.Pages;

namespace DeviceRunners.VisualRunners.Maui;

public partial class VisualRunnerAppShell : Shell
{
	public VisualRunnerAppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("testrunner/assembly", typeof(TestAssemblyPage));
		Routing.RegisterRoute("testrunner/assembly/result", typeof(TestResultPage));
	}
}
