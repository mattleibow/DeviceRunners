using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace DeviceRunners.UIAutomation.Selenium;

public class EdgeSeleniumAutomatedAppOptions : SeleniumAutomatedAppOptions
{
	public EdgeSeleniumAutomatedAppOptions(string key, EdgeOptions driverOptions, IReadOnlyList<IAutomatedAppCommand> commands)
		: base(key, driverOptions, new EdgeSeleniumDriverFactory(), new SeleniumByFactory(), commands)
	{
	}

	public new EdgeOptions DriverOptions => (EdgeOptions)base.DriverOptions;
}
