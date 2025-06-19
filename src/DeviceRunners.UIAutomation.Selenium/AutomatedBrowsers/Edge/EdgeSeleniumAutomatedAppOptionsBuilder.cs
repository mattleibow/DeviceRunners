using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace DeviceRunners.UIAutomation.Selenium;

public class EdgeSeleniumAutomatedAppOptionsBuilder : SeleniumAutomatedAppOptionsBuilder
{
	public EdgeSeleniumAutomatedAppOptionsBuilder(string key)
		: base(key, new EdgeOptions())
	{
	}

	public new EdgeOptions DriverOptions => (EdgeOptions)base.DriverOptions;

	public override SeleniumAutomatedAppOptions Build() =>
		new EdgeSeleniumAutomatedAppOptions(Key, DriverOptions, Commands);
}
