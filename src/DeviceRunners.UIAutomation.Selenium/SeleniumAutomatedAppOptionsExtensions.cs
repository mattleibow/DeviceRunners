using System.Diagnostics.CodeAnalysis;

namespace DeviceRunners.UIAutomation.Selenium;

public static class SeleniumAutomatedAppOptionsExtensions
{
	internal const string DriverOptionPrefix = "devicerunners:";
	internal const string InitialUrlDriverOption = "initialUrl";

	private static bool TryGetCapability(this SeleniumAutomatedAppOptions options, string name, [NotNullWhen(true)] out object? value)
	{
		var obj = options.DriverOptions.ToCapabilities().GetCapability(DriverOptionPrefix + name);
		if (obj is null)
		{
			value = null;
			return false;
		}

		value = obj;
		return true;
	}

	private static bool TryGetStringCapability(this SeleniumAutomatedAppOptions options, string name, [NotNullWhen(true)] out string? value)
	{
		if (!options.TryGetCapability(name, out var objVal) ||
			objVal.ToString() is not string strVal ||
			string.IsNullOrWhiteSpace(strVal))
		{
			value = null;
			return false;
		}

		value = strVal;
		return true;
	}

	public static bool TryGetInitialUrl(this SeleniumAutomatedAppOptions options, [NotNullWhen(true)] out string? initialUrl)
	{
		if (!options.TryGetStringCapability(InitialUrlDriverOption, out var url))
		{
			initialUrl = null;
			return false;
		}

		initialUrl = url;
		return true;
	}
}
